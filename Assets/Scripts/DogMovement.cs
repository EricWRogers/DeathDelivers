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

public Transform AnchorL;
public Transform AnchorR;

private float targetTurn;

private Quaternion cameraBaseRotation;
private Vector3 cameraBasePosition;

private bool quickTurning = false;
private float quickTurnTimer = 0f;
private float quickTurnDirection = 0f;

private Transform activeAnchor;
private Transform originalParent;
private Transform originalCameraParent;

private Vector3 activeAnchorLockedPosition;

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

    originalParent = transform.parent;

    // Save anchor exact world position
    activeAnchorLockedPosition = activeAnchor.position;

    // Remove anchor from the Dog
    activeAnchor.SetParent(null,true);

    // Make Dog a child of anchor
    transform.SetParent(activeAnchor,true);

    if (cameraConnect != null)
    {
        originalCameraParent = cameraConnect.parent;

        cameraConnect.SetParent(activeAnchor,true);

        quickTurnCameraWorldRotation = cameraConnect.rotation;

        quickTurnCameraWorldPosition = activeAnchorLockedPosition;
    }
}

void DoQuickTurn()
{
    // Keep anchor locked
    activeAnchor.position = activeAnchorLockedPosition;

    // quick turn speed
    float quickTurnSpeed = speed * dogBase.turnSpeed * quickTurnMultiplier;

    float rotationAmount = quickTurnDirection * quickTurnSpeed * Time.deltaTime;

    activeAnchor.Rotate(0f,rotationAmount,0f);
}

void EndQuickTurn()
{
    quickTurning = false;

    // Restore cam og parent
    if (cameraConnect != null)
    {
        cameraConnect.SetParent(originalCameraParent,true);
    }

    // Restore dof og parent
    transform.SetParent(originalParent,true);

    // Put anchor back under big D
    activeAnchor.SetParent(transform,true);

    activeAnchor = null;
    originalParent = null;
    originalCameraParent = null;

    turn = 0f;
}

void LateUpdate()
{
    if (quickTurning && cameraConnect != null)
    {
    // Keep camera at the anchor
    cameraConnect.position = quickTurnCameraWorldPosition;

        Vector3 cameraEuler = cameraConnect.rotation.eulerAngles;

        Vector3 lockedEuler = quickTurnCameraWorldRotation.eulerAngles;

        cameraConnect.rotation = Quaternion.Euler(cameraEuler.x,cameraEuler.y,lockedEuler.z);
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