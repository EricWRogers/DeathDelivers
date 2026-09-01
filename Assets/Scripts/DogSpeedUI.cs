using UnityEngine;
using TMPro;

public class DogSpeedUI : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public TMP_Text speedText;

    void Update()
    {
        if (dogMovement == null || speedText == null)
        {
            return;
        }

        speedText.text = "Speed: " + dogMovement.speed.ToString("0");
    }
}