
using UnityEngine;
using UnityEngine.InputSystem;

public class DogCameraMovement : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public Transform cameraConnect;
    public Camera playerCamera;

    [Header("Position Follow")]
    public float positionSmoothness = 8f;
    public Vector3 positionOffset;

    [Header("Camera Turning")]
    public float cameraTurnAmount = 5f;
    public float cameraSideShift = 0.5f;
    public float cameraSmoothness = 8f;

    [Header("Drift Camera")]
    public float driftCameraTurnAmount = 15f;

    [Header("Camera Flip")]
    public float flipCameraSmoothness = 8f;

    [Header("Camera Collision")]
    public float collisionRadius = 0.25f;
    public float collisionPadding = 0.1f;
    public float collisionSmoothness = 12f;
    public LayerMask collisionLayers = ~0;

    private Quaternion cameraBaseRotation;
    private Vector3 cameraBasePosition;

    private float cameraFlipAngle = 0f;

    private Vector3 cameraLocalOffset;
    private float currentCollisionDistance;

    void Start()
    {
        if (cameraConnect != null)
        {
            cameraBaseRotation = cameraConnect.localRotation;
            cameraBasePosition = cameraConnect.localPosition;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null && cameraConnect != null)
        {
            cameraLocalOffset = cameraConnect.InverseTransformPoint(
                playerCamera.transform.position
            );

            currentCollisionDistance = cameraLocalOffset.magnitude;
        }
    }

    void LateUpdate()
    {
        if (dogMovement == null || cameraConnect == null)
            return;

        bool shiftHeld = Keyboard.current != null &&
            (Keyboard.current.leftShiftKey.isPressed ||
             Keyboard.current.rightShiftKey.isPressed);

        Transform dog = dogMovement.transform;

        Vector3 targetPosition = dog.position + positionOffset;

        cameraConnect.position = Vector3.Lerp(
            cameraConnect.position,
            targetPosition,
            positionSmoothness * Time.deltaTime
        );

        float dogX = NormalizeAngle(dog.localEulerAngles.x);
        float dogZ = NormalizeAngle(dog.localEulerAngles.z);
        float dogY = NormalizeAngle(dog.eulerAngles.y);

        Quaternion tiltCorrection = Quaternion.Euler(
            -dogX,
            0f,
            -dogZ
        );

        Quaternion dogYRotation = Quaternion.Euler(
            0f,
            dogY,
            0f
        );

        float turnAmount = 0f;

        if (dogMovement.dogBase != null &&
            dogMovement.dogBase.turnSpeed != 0f)
        {
            turnAmount =
                dogMovement.turn /
                dogMovement.dogBase.turnSpeed;

            turnAmount = Mathf.Clamp(turnAmount, -1f, 1f);
        }

        float cameraAngle = -turnAmount * cameraTurnAmount;

        if (dogMovement.IsDrifting())
        {
            cameraAngle += -turnAmount * driftCameraTurnAmount;
        }

        cameraFlipAngle = Mathf.Lerp(
            cameraFlipAngle,
            shiftHeld ? 180f : 0f,
            flipCameraSmoothness * Time.deltaTime
        );

        Quaternion flipRotation = Quaternion.Euler(
            0f,
            cameraFlipAngle,
            0f
        );

        Quaternion targetRotation =
            dogYRotation *
            flipRotation *
            cameraBaseRotation;

        targetRotation *= tiltCorrection;

        targetRotation *= Quaternion.Euler(
            0f,
            0f,
            cameraAngle
        );

        cameraConnect.rotation = Quaternion.Slerp(
            cameraConnect.rotation,
            targetRotation,
            cameraSmoothness * Time.deltaTime
        );

        float targetX =
            cameraBasePosition.x +
            turnAmount * cameraSideShift;

        Vector3 targetLocalPosition = new Vector3(
            targetX,
            cameraBasePosition.y,
            cameraBasePosition.z
        );

        // Preserve the original cameraConnect position behavior.
        // Only apply the original side shift if needed.
        // Collision never modifies cameraConnect.
        
        if (playerCamera != null)
            HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        Vector3 origin = cameraConnect.position;

        Vector3 desiredPosition = cameraConnect.TransformPoint(
            cameraLocalOffset
        );

        Vector3 direction = desiredPosition - origin;
        float desiredDistance = direction.magnitude;

        if (desiredDistance <= 0.001f)
            return;

        direction.Normalize();

        float targetDistance = desiredDistance;

        if (Physics.SphereCast(
            origin,
            collisionRadius,
            direction,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore
        ))
        {
            targetDistance = Mathf.Max(
                0f,
                hit.distance - collisionPadding
            );
        }

        // Move in immediately when blocked, smoothly return when clear.
        if (targetDistance < currentCollisionDistance)
        {
            currentCollisionDistance = targetDistance;
        }
        else
        {
            currentCollisionDistance = Mathf.Lerp(
                currentCollisionDistance,
                targetDistance,
                collisionSmoothness * Time.deltaTime
            );
        }

        playerCamera.transform.position =
            origin + direction * currentCollisionDistance;
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}
