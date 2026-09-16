using UnityEngine;

public class SpeedometerUI : MonoBehaviour
{
    [Header("UI Reference")]
    public RectTransform boneNeedle; 

    [Header("Dog Script Reference")]
    public DogMovement dogScript; 

    [Header("Precise Angle Mapping")]
    [Tooltip("Enter the exact Z rotation for each 10 mph step.\n[0] = 0 mph, [1] = 10 mph, [2] = 20 mph, etc.")]
    public float[] tickAngles = new float[11]; // Stores angles for 0, 10, 20... up to 100 mph

    void Start()
    {
        if (boneNeedle != null && tickAngles.Length > 0)
        {
            
            boneNeedle.localEulerAngles = new Vector3(0, 0, tickAngles[0]);
        }
    }

    void Update()
    {
        float currentSpeed = 0f;

        if (dogScript != null)
        {
            currentSpeed = dogScript.speed;
        }

        if (boneNeedle != null && tickAngles.Length >= 2)
        {
            float targetAngle = GetAngleFromSpeed(currentSpeed);
            boneNeedle.localEulerAngles = new Vector3(0, 0, targetAngle);
        }
    }

    float GetAngleFromSpeed(float speed)
    {
        
        float maxSupportedSpeed = (tickAngles.Length - 1) * 10f;
        speed = Mathf.Clamp(speed, 0f, maxSupportedSpeed);

        
        int lowerIndex = Mathf.FloorToInt(speed / 10f);
        int upperIndex = Mathf.CeilToInt(speed / 10f);

        
        if (upperIndex >= tickAngles.Length) upperIndex = tickAngles.Length - 1;
        if (lowerIndex >= tickAngles.Length) lowerIndex = tickAngles.Length - 1;

       
        float remainderSpeed = speed % 10f;
        float t = remainderSpeed / 10f;

       
        if (lowerIndex == upperIndex) t = 0f; 

        
        return Mathf.Lerp(tickAngles[lowerIndex], tickAngles[upperIndex], t);
    }
}
