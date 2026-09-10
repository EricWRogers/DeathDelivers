using UnityEngine;
using TMPro;
using System.Collections;

public class TotalSoulCountUI : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;

    [Header("Soul Counter Digits")]
    public TMP_Text thousandsDigit;
    public TMP_Text hundredsDigit;
    public TMP_Text tensDigit;
    public TMP_Text onesDigit;

    [Header("Animation")]
    public float rollSpeed = 0.05f;

    private int displayedValue = 0;
    private Coroutine counterCoroutine;

    private void Start()
    {
        UpdateDisplay(0);
    }

    private void Update()
    {
        if (dogMovement == null)
            return;

        int targetValue = Mathf.Clamp(
            Mathf.FloorToInt(dogMovement.totalSoulCount),
            0,
            9999
        );

        if (targetValue != displayedValue)
        {
            if (counterCoroutine != null)
                StopCoroutine(counterCoroutine);

            counterCoroutine = StartCoroutine(RollToValue(targetValue));
        }
    }

    private IEnumerator RollToValue(int targetValue)
    {
        while (displayedValue != targetValue)
        {
            if (displayedValue < targetValue)
                displayedValue++;
            else
                displayedValue--;

            UpdateDisplay(displayedValue);

            yield return new WaitForSeconds(rollSpeed);
        }

        counterCoroutine = null;
    }

    private void UpdateDisplay(int value)
    {
        int thousands = (value / 1000) % 10;
        int hundreds = (value / 100) % 10;
        int tens = (value / 10) % 10;
        int ones = value % 10;

        thousandsDigit.text = thousands.ToString();
        hundredsDigit.text = hundreds.ToString();
        tensDigit.text = tens.ToString();
        onesDigit.text = ones.ToString();
    }
}