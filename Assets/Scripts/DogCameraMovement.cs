using UnityEngine;

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

    private Quaternion cameraBaseRotation;
    private Vector3 cameraBasePosition;

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

        Transform dog = dogMovement.transform;

        // --------------------------------
        // POSITION
        // --------------------------------

        Vector3 targetPosition = dog.position + positionOffset;

        cameraConnect.position = Vector3.Lerp(
            cameraConnect.position,
            targetPosition,
            positionSmoothness * Time.deltaTime
        );

        // --------------------------------
        // ROTATION
        // --------------------------------

        float dogX = NormalizeAngle(
            dog.localEulerAngles.x
        );

        float dogZ = NormalizeAngle(
            dog.localEulerAngles.z
        );

        float dogY = NormalizeAngle(
            dog.eulerAngles.y
        );

        // Counter dog's X and Z tilt
        Quaternion tiltCorrection = Quaternion.Euler(
            -dogX,
            0f,
            -dogZ
        );

        // Follow dog's Y rotation
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

        // Camera roll while turning
        float cameraAngle =
            -turnAmount * cameraTurnAmount;

        // Start with dog's Y rotation
        Quaternion targetRotation =
            dogYRotation *
            cameraBaseRotation;

        // Counter dog's X and Z tilt
        targetRotation *= tiltCorrection;

        // Add camera roll
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

        // --------------------------------
        // SIDE SHIFT
        // --------------------------------

        float targetX =
            cameraBasePosition.x +
            turnAmount * cameraSideShift;

        Vector3 targetLocalPosition = new Vector3(
            targetX,
            cameraBasePosition.y,
            cameraBasePosition.z
        );

        // If you want XYZ to follow the dog,
        // don't overwrite the position here.
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
