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
public float maxTiltAngle = 15f;
public float uprightSmoothness = 8f;

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
private float quickTurnDirection = 0f;

private Transform activeAnchor;
private Transform originalParent;

private Vector3 activeAnchorLockedPosition;
private Vector3 activeAnchorOriginalLocalScale;
private Vector3 dogOriginalLocalScale;

private bool touchingWall = false;

private Vector3 quickTurnCameraLocalPosition;
private Quaternion quickTurnCameraLocalRotation;
private Transform quickTurnCameraParent;

void Start()
{
    speed = dogBase.startBoost;
    soulCount = 30f;

    dogOriginalLocalScale = transform.localScale;
    
    if (AnchorL != null)
    {
        AnchorL.localScale = AnchorL.localScale;
    }

    if (AnchorR != null)
    {
        AnchorR.localScale = AnchorR.localScale;
    }
    
    if (cameraConnect != null)
    {
        cameraBaseRotation = cameraConnect.localRotation;
        cameraBasePosition = cameraConnect.localPosition;

        quickTurnCameraParent = cameraConnect.parent;
    }
}

void Update()
{
    soulCount -= dogBase.soulConsump * Time.deltaTime;
    soulCount = Mathf.Max(soulCount,0f);

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
    if (!touchingWall)
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

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

    transform.Rotate(Vector3.up * turn * Time.deltaTime);

    KeepDogUpright();

    UpdateNormalCamera();
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

    originalParent = transform.parent;

    // Save exact original scales
    dogOriginalLocalScale = transform.localScale;
    activeAnchorOriginalLocalScale = activeAnchor.localScale;

    // Save anchor exact world position
    activeAnchorLockedPosition = activeAnchor.position;

    // Remove anchor from the Dog
    activeAnchor.SetParent(null,true);

    // Restore exact anchor scale
    activeAnchor.localScale = activeAnchorOriginalLocalScale;

    // Make Dog a child of anchor
    transform.SetParent(activeAnchor,true);

    // Restore exact Dog scale
    transform.localScale = dogOriginalLocalScale;

    // Save camera transform
    if (cameraConnect != null)
    {
        quickTurnCameraLocalPosition = cameraConnect.localPosition;
        quickTurnCameraLocalRotation = cameraConnect.localRotation;
    }
}

void DoQuickTurn()
{
    // Keep anchor locked
    activeAnchor.position = activeAnchorLockedPosition;

    // Keep anchor scale unchanged
    activeAnchor.localScale = activeAnchorOriginalLocalScale;

    // quick turn speed
    float quickTurnSpeed = speed * dogBase.turnSpeed * quickTurnMultiplier;

    float rotationAmount = quickTurnDirection * quickTurnSpeed * Time.deltaTime;

    activeAnchor.Rotate(0f,rotationAmount,0f);

    KeepDogUpright();

    if (cameraConnect != null)
    {
        cameraConnect.localPosition = quickTurnCameraLocalPosition;
        cameraConnect.localRotation = quickTurnCameraLocalRotation;
    }
}

void EndQuickTurn()
{
    quickTurning = false;

    transform.SetParent(originalParent,true);

    transform.localScale = dogOriginalLocalScale;

    activeAnchor.SetParent(transform,true);

    activeAnchor.localScale = activeAnchorOriginalLocalScale;

    // Restore came original local transform
    if (cameraConnect != null)
    {
        cameraConnect.SetParent(quickTurnCameraParent,true);

        cameraConnect.localPosition = quickTurnCameraLocalPosition;
        cameraConnect.localRotation = quickTurnCameraLocalRotation;
    }

    activeAnchor = null;
    originalParent = null;

    turn = 0f;

    KeepDogUpright();
}

void KeepDogUpright()
{
    Vector3 currentEuler = transform.rotation.eulerAngles;

    float xAngle = NormalizeAngle(currentEuler.x);
    float zAngle = NormalizeAngle(currentEuler.z);

    xAngle = Mathf.Clamp(xAngle,-maxTiltAngle,maxTiltAngle);
    zAngle = Mathf.Clamp(zAngle,-maxTiltAngle,maxTiltAngle);

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

void LateUpdate()
{
    if (cameraConnect == null)
    {
        return;
    }

    if (quickTurning)
    {
        // Keep camera stable during quick turn.
        cameraConnect.localPosition = quickTurnCameraLocalPosition;
        cameraConnect.localRotation = quickTurnCameraLocalRotation;

        return;
    }

    // Get the dog's local tilt
    float dogX = NormalizeAngle(transform.localEulerAngles.x);
    float dogZ = NormalizeAngle(transform.localEulerAngles.z);

    // Counter the dog's X and Z tilt.
    Quaternion tiltCorrection = Quaternion.Euler(-dogX,0f,-dogZ);

    float turnAmount = 0f;

    if (dogBase.turnSpeed != 0f)
    {
        turnAmount = turn / dogBase.turnSpeed;
        turnAmount = Mathf.Clamp(turnAmount,-1f,1f);
    }

    float cameraAngle = -turnAmount * cameraTurnAmount;

    Quaternion targetRotation = cameraBaseRotation * tiltCorrection;

    targetRotation *= Quaternion.Euler(0f,0f,cameraAngle);

    cameraConnect.localRotation = Quaternion.Slerp(
        cameraConnect.localRotation,
        targetRotation,
        cameraSmoothness * Time.deltaTime
    );

    // cam x pos shift
    float targetX = cameraBasePosition.x + (turnAmount * cameraSideShift);

    Vector3 targetCameraPosition = new Vector3(targetX,cameraBasePosition.y,cameraBasePosition.z);

    cameraConnect.localPosition = Vector3.Lerp(
        cameraConnect.localPosition,
        targetCameraPosition,
        cameraSmoothness * Time.deltaTime
    );
}

void UpdateNormalCamera()
{
    // Camera is updated in LateUpdate.
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