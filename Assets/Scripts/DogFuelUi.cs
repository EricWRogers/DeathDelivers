using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DogFuelUI : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public Image fuelBar;
    public TMP_Text fuelText;

    [Header("Fuel Settings")]
    public float maxFuel = 100f;

    private void Update()
    {
        if (dogMovement == null)
            return;

        // Convert soulCount into a percentage from 0 to 1
        float fuelPercent = Mathf.Clamp01(
            dogMovement.soulCount / maxFuel
        );

        // Update the fuel bar
        if (fuelBar != null)
        {
            fuelBar.fillAmount = fuelPercent;
        }

        // Update the optional text
        if (fuelText != null)
        {
            fuelText.text =
                Mathf.RoundToInt(dogMovement.soulCount) + "/100";
        }
    }
}