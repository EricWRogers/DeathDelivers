using UnityEngine;
using UnityEngine.InputSystem;

public class DogMovement : MonoBehaviour
{
    public DogBase dogBase;

    [Header("Soul & Speed")]
    public float soulCount;
    public float speed;

    [Header("Turning")]
    public float turn;
    public float turnSmoothness = 8f;

    [Header("Camera")]
    public Transform cameraConnect;
    public float cameraTurnAmount = 5f;
    public float cameraSideShift = 0.5f;
    public float cameraSmoothness = 8f;

    [Header("Quick Turn")]
    public float quickTurnDuration = 0.25f;
    public float quickTurnMultiplier = 1f;

    private float targetTurn;

    private Quaternion cameraBaseRotation;
    private Vector3 cameraBasePosition;

    private bool quickTurning = false;
    private float quickTurnTimer = 0f;
    private float quickTurnDirection = 0f;

    // World rotation of the camera when the quick turn begins
    private Quaternion quickTurnCameraWorldRotation;
    private Vector3 quickTurnCameraWorldPosition;


    void Start()
    {
        speed = dogBase.startBoost;
        soulCount = 30f;

        if (cameraConnect != null)
        {
            cameraBaseRotation = cameraConnect.localRotation;
            cameraBasePosition = cameraConnect.localPosition;
        }
    }


    void Update()
    {
        soulCount -= dogBase.soulConsump * Time.deltaTime;
        soulCount = Mathf.Max(soulCount, 0f);

        speed = soulCount * dogBase.speed;

        // quick turn input
        if (!quickTurning)
        {
            if (Keyboard.current.dKey.isPressed &&
                Keyboard.current.spaceKey.isPressed)
            {
                StartQuickTurn(1f);
            }
            else if (Keyboard.current.aKey.isPressed &&
                     Keyboard.current.spaceKey.isPressed)
            {
                StartQuickTurn(-1f);
            }
        }
        
        if (quickTurning)
        {
            DoQuickTurn();
            return;
        }

        // move forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // turn input
        targetTurn = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            targetTurn = -dogBase.turnSpeed;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            targetTurn = dogBase.turnSpeed;
        }

        // norm turn
        turn = Mathf.Lerp(turn,targetTurn,turnSmoothness * Time.deltaTime);

        transform.Rotate( Vector3.up * turn * Time.deltaTime);

        UpdateNormalCamera();
    }

    void StartQuickTurn(float direction)
    {
        quickTurning = true;

        quickTurnTimer = 0f;

        quickTurnDirection = direction;

        turn = 0f;

        if (cameraConnect != null)
        {
            quickTurnCameraWorldRotation = cameraConnect.rotation;

            quickTurnCameraWorldPosition = cameraConnect.position;
        }
    }

    void DoQuickTurn()
    {
        quickTurnTimer += Time.deltaTime;

        // quick turn speed
        float quickTurnSpeed = speed * dogBase.turnSpeed * quickTurnMultiplier;

        float rotationAmount = quickTurnDirection * quickTurnSpeed * Time.deltaTime;

        transform.Rotate(0f,rotationAmount,0f);

        if (quickTurnTimer >= quickTurnDuration)
        {
            quickTurning = false;
            turn = 0f;
        }
    }
    void LateUpdate()
    {
        if (quickTurning && cameraConnect != null)
        {
            // Force the camera to keep the exact same
            cameraConnect.rotation =
                quickTurnCameraWorldRotation;

            cameraConnect.position =
                quickTurnCameraWorldPosition;
        }
    }

    void UpdateNormalCamera()
    {
        if (cameraConnect == null)
        {
            return;
        }

        // turning 
        float turnAmount = 0f;

        if (dogBase.turnSpeed != 0f)
        {
            turnAmount = turn / dogBase.turnSpeed;
            turnAmount = Mathf.Clamp(turnAmount,-1f,1f);
        }

        // cam rot
        float cameraAngle = -turnAmount * cameraTurnAmount;
        Quaternion targetCameraRotation = cameraBaseRotation * Quaternion.Euler(0f,0f,cameraAngle);

        // cam x pos shift
        float targetX = cameraBasePosition.x + (turnAmount * cameraSideShift);
        Vector3 targetCameraPosition = new Vector3(targetX,cameraBasePosition.y,cameraBasePosition.z);

        //smooth cam rot
        cameraConnect.localRotation = Quaternion.Slerp(cameraConnect.localRotation,targetCameraRotation,cameraSmoothness * Time.deltaTime);

        // smooth cam pos
        cameraConnect.localPosition = Vector3.Lerp(cameraConnect.localPosition,targetCameraPosition,cameraSmoothness * Time.deltaTime);
    }
}