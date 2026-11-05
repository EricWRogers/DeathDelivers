
using UnityEngine;
using UnityEngine.InputSystem;

public class DogCameraMovement : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public Transform cameraConnect;

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

    private Quaternion cameraBaseRotation;
    private Vector3 cameraBasePosition;

    private float cameraFlipAngle = 0f;

    void Start()
    {
        if (cameraConnect != null)
        {
            cameraBaseRotation = cameraConnect.localRotation;
            cameraBasePosition = cameraConnect.localPosition;
        }
    }

    void LateUpdate()
    {
        if (dogMovement == null || cameraConnect == null)
        {
            return;
        }

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

        if (dogMovement.dogBase.turnSpeed != 0f)
        {
            turnAmount =
                dogMovement.turn /
                dogMovement.dogBase.turnSpeed;

            turnAmount = Mathf.Clamp(
                turnAmount,
                -1f,
                1f
            );
        }

        float cameraAngle = -turnAmount * cameraTurnAmount;

        // Add extra camera banking while drifting.
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
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}
