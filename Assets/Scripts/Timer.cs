using UnityEngine;
using stroke = UnityEngine.UI; // Using standard UI for RawImage
using TMPro;

public class Timer : MonoBehaviour
{
    public float maxTime = 10f;
    private float timeRemaining;

    [Header("UI Elements")]
    public TMP_Text timerText;
    
    [Header("Raw Image Fill Settings")]
    public RectTransform maskContainer; // Drag the 'Mask_Container' here
    private float maxHeight;            // Stores the starting height of the mask

    void Start()
    {
        timeRemaining = maxTime;

        // Remember the original maximum height of the mask container
        if (maskContainer != null)
        {
            maxHeight = maskContainer.rect.height;
        }
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // 1. Update UI Text
            if (timerText != null)
                timerText.text = "Time: " + timeRemaining.ToString("0.00");

            // 2. Update Raw Image Mask Height
            if (maskContainer != null)
            {
                // Calculate the percentage of time left (0.0 to 1.0)
                float timePercentage = timeRemaining / maxTime;

                // Adjust the height of the masking container dynamically
                maskContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxHeight * timePercentage);
            }
        }
        else
        {
            timeRemaining = 0;
            if (timerText != null) timerText.text = "Time: 0.00";
            if (maskContainer != null) maskContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }
    }
}
