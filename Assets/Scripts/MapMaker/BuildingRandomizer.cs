using UnityEngine;

public class BuildingRandomizer : MonoBehaviour
{
    [Header("Y Positions")]
    public float yPosition1 = 0f;
    public float yPosition2 = 1f;
    public float yPosition3 = 2f;

    [Header("Y Heights")]
    public float yHeight1 = 1f;
    public float yHeight2 = 2f;
    public float yHeight3 = 3f;

    [Header("Colors")]
    public Color[] colors;

    // Fail-safe color
    private readonly Color failSafeRed = Color.red;

    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();

        // Always start with red
        if (renderer != null)
        {
            renderer.material.color = failSafeRed;
        }

        // Run the normal randomizer
        RandomizeBuilding();

        // Check if the material is still red
        if (renderer != null &&
            renderer.material.color == failSafeRed)
        {
            // Try the color randomizer again
            RandomizeColor();
        }
    }

    void RandomizeBuilding()
    {
        // Choose one of the 3 building variations
        int variation = Random.Range(0, 3);

        float randomY;
        float randomHeight;

        switch (variation)
        {
            case 0:
                randomY = yPosition1;
                randomHeight = yHeight1;
                break;

            case 1:
                randomY = yPosition2;
                randomHeight = yHeight2;
                break;

            default:
                randomY = yPosition3;
                randomHeight = yHeight3;
                break;
        }

        // Set local Y position
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            randomY,
            transform.localPosition.z
        );

        // Set local Y size
        transform.localScale = new Vector3(
            transform.localScale.x,
            randomHeight,
            transform.localScale.z
        );

        // Try to assign a random color
        RandomizeColor();
    }

    void RandomizeColor()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null)
            return;

        if (colors == null || colors.Length == 0)
            return;

        Color randomColor =
            colors[Random.Range(0, colors.Length)];

        renderer.material.color = randomColor;
    }
}