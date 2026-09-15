using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DogFuelUI : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public RectTransform fuelBar;
    public TMP_Text fuelText;

    [Header("Fuel Settings")]
    public float maxFuel = 100f;
    public float maxBarWidth = 400f;

    private void Update()
    {
        if (dogMovement == null || fuelBar == null)
            return;

        float fuelPercent = Mathf.Clamp01(
            dogMovement.soulCount / maxFuel
        );

        fuelBar.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            maxBarWidth * fuelPercent
        );

        if (fuelText != null)
        {
            fuelText.text =
                Mathf.RoundToInt(dogMovement.soulCount) + "/100";
        }
    }
}