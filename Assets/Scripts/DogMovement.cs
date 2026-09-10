using UnityEngine;
using UnityEngine.InputSystem;

public class DogMovement : MonoBehaviour
{
    public DogBase dogBase;

    [Header("Soul & Speed")]
    public float soulCount;
    public int totalSoulCount;
    public float speed;

    [Header("Turning")]
    public float turn;
    public float turnSmoothness = 8f;
    public float maxTiltAngle = 15f;
    public float uprightSmoothness = 8f;

    [Header("Quick Turn")]
    public float quickTurnDuration = 0.25f;
    public float quickTurnMultiplier = 1f;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("FireWorks")]
    private int fireworks = 0;
    private bool hasFireWork = false;
    public float fireworkJump = 5f;
    public float fireworkDuration = 1f;
    private float fireworkTimeRemaining = 0f;

    public Transform AnchorL;
    public Transform AnchorR;

    private float targetTurn;

    private bool quickTurning = false;
    private float quickTurnDirection = 0f;

    private Transform activeAnchor;

    private Vector3 activeAnchorLockedPosition;
    private Vector3 dogPositionRelativeToAnchor;

    private bool touchingWall = false;

    void Start()
    {
        speed = dogBase.startBoost;
        soulCount = 30f;
        totalSoulCount += 30;
    }

    void Update()
    {
        soulCount -= dogBase.soulConsump * Time.deltaTime;
        soulCount = Mathf.Max(soulCount, 0f);

        speed = soulCount * dogBase.speed;

        if (Keyboard.current.fKey.wasPressedThisFrame){
            UseFireWork();
        }

        if (fireworkTimeRemaining > 0f){
            fireworkTimeRemaining -= Time.deltaTime;
            fireworkJump++;
            transform.Translate(Vector3.up * fireworkJump * Time.deltaTime, Space.World);
        }

        // Quick turn input
        if (!quickTurning)
        {
            if (Keyboard.current.dKey.isPressed &&
                Keyboard.current.spaceKey.isPressed &&
                IsGrounded())
            {
                StartQuickTurn(1f);
            }
            else if (Keyboard.current.aKey.isPressed &&
                     Keyboard.current.spaceKey.isPressed &&
                     IsGrounded())
            {
                StartQuickTurn(-1f);
            }
        }

        if (quickTurning)
        {
            if (Keyboard.current.spaceKey.isPressed)
            {
                DoQuickTurn();
                return;
            }
            else
            {
                EndQuickTurn();
            }
        }

        // Move forward
        if (!touchingWall)
        {
            transform.Translate(
                Vector3.forward * speed * Time.deltaTime
            );
        }

        // Turn input
        targetTurn = 0f;

        if (Keyboard.current.aKey.isPressed)
        {
            targetTurn = -dogBase.turnSpeed;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            targetTurn = dogBase.turnSpeed;
        }

        // Normal turn
        turn = Mathf.Lerp(
            turn,
            targetTurn,
            turnSmoothness * Time.deltaTime
        );

        transform.Rotate(
            Vector3.up * turn * Time.deltaTime
        );

        KeepDogUpright();
    }

    void StartQuickTurn(float direction)
    {
        quickTurning = true;
        quickTurnDirection = direction;
        turn = 0f;

        if (direction > 0f)
        {
            activeAnchor = AnchorR;
        }
        else
        {
            activeAnchor = AnchorL;
        }

        if (activeAnchor == null)
        {
            quickTurning = false;
            return;
        }

        activeAnchorLockedPosition = activeAnchor.position;

        dogPositionRelativeToAnchor =
            transform.position - activeAnchorLockedPosition;
    }

    void DoQuickTurn()
    {
        if (activeAnchor == null)
            return;

        float quickTurnSpeed =
            speed *
            dogBase.turnSpeed *
            quickTurnMultiplier;

        float rotationAmount =
            quickTurnDirection *
            quickTurnSpeed *
            Time.deltaTime;

        Quaternion rotation =
            Quaternion.Euler(0f, rotationAmount, 0f);

        dogPositionRelativeToAnchor =
            rotation * dogPositionRelativeToAnchor;

        transform.position =
            activeAnchorLockedPosition +
            dogPositionRelativeToAnchor;

        transform.rotation =
            rotation * transform.rotation;

        KeepDogUpright();
    }

    void EndQuickTurn()
    {
        quickTurning = false;

        activeAnchor = null;

        turn = 0f;

        KeepDogUpright();
    }

    bool IsGrounded()
    {
        Collider col = GetComponent<Collider>();

        if (col == null)
            return false;

        Vector3 origin = col.bounds.center;
        origin.y = col.bounds.min.y + 0.05f;

        return Physics.Raycast(
            origin,
            Vector3.down,
            groundCheckDistance + 0.05f,
            groundLayer
        );
    }

    void KeepDogUpright()
    {
        Vector3 currentEuler = transform.rotation.eulerAngles;

        float xAngle = NormalizeAngle(currentEuler.x);
        float zAngle = NormalizeAngle(currentEuler.z);

        xAngle = Mathf.Clamp(
            xAngle,
            -maxTiltAngle,
            maxTiltAngle
        );

        zAngle = Mathf.Clamp(
            zAngle,
            -maxTiltAngle,
            maxTiltAngle
        );

        Quaternion targetRotation = Quaternion.Euler(
            xAngle,
            currentEuler.y,
            zAngle
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            uprightSmoothness * Time.deltaTime
        );
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    public bool IsQuickTurning()
    {
        return quickTurning;
    }

    public void AddFirework()
    {
        fireworks++;
        hasFireWork = true;

        
    }

    public void UseFireWork()
    {
        if (fireworks >= 1)
        {
            fireworks--;
            hasFireWork = fireworks > 0;
            fireworkTimeRemaining = fireworkDuration;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Wall")
        {
            touchingWall = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == "Wall")
        {
            touchingWall = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Wall")
        {
            touchingWall = false;
        }
    }
}
