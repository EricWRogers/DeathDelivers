using UnityEngine;

public class Speedometer : MonoBehaviour
{
    private const float MAX_SPEED_ANGLE = -20f;
    private const float ZERO_SPEED_ANGLE = 210f;

    private Transform needleTransform;

    [Header("Dog Movement")]
    public DogMovement dogMovement;

    [Header("Speed Settings")]
    public float speedMax = 100f;

    private void Awake()
    {
        needleTransform = transform.Find("needle");

        if (needleTransform == null)
        {
            Debug.LogError("Could not find 'needle' as a child of the Speedometer!");
        }

        if (dogMovement == null)
        {
            Debug.LogError("DogMovement has not been assigned to the Speedometer!");
        }
    }

    private void Update()
    {
        if (needleTransform == null || dogMovement == null)
        {
            return;
        }

        // Get the current speed from DogMovement
        float speed = dogMovement.speed;

        // Calculate the needle rotation
        float speedRotation = GetSpeedRotation(speed);

        // Rotate the needle
        needleTransform.eulerAngles = new Vector3(
            0f,
            0f,
            speedRotation
        );
    }

    private float GetSpeedRotation(float speed)
    {
        float totalAngleSize = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;

        // Convert speed into a value between 0 and 1
        float speedNormalized = speed / speedMax;

        // Prevent the needle from going past the speedometer
        speedNormalized = Mathf.Clamp01(speedNormalized);

        return ZERO_SPEED_ANGLE - speedNormalized * totalAngleSize;
    }
}