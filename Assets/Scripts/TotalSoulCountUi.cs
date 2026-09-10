using UnityEngine;
using TMPro;

public class TotalSoulCountUI : MonoBehaviour
{
    [Header("References")]
    public DogMovement dogMovement;
    public TMP_Text totalSoulText;

    private void Update()
    {
        if (dogMovement == null || totalSoulText == null)
            return;

        totalSoulText.text = "Souls Consumed: "+ dogMovement.totalSoulCount.ToString();
    }
}