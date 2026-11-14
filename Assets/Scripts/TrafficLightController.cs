using System.Collections;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [Header("Bulb Renderers")]
    [SerializeField] private Renderer redBulb;
    [SerializeField] private Renderer yellowBulb;
    [SerializeField] private Renderer greenBulb;

    [Header("Timing")]
    [SerializeField] private float greenDuration = 5f;
    [SerializeField] private float yellowDuration = 2f;
    [SerializeField] private float redDuration = 5f;

    private Material redMaterial;
    private Material yellowMaterial;
    private Material greenMaterial;

    private static readonly int EmissionColor =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        redMaterial = redBulb.material;
        yellowMaterial = yellowBulb.material;
        greenMaterial = greenBulb.material;
    }

    private void Start()
    {
        StartCoroutine(CycleLights());
    }

    private IEnumerator CycleLights()
    {
        while (true)
        {
            SetLights(false, false, true);
            yield return new WaitForSeconds(greenDuration);

            SetLights(false, true, false);
            yield return new WaitForSeconds(yellowDuration);

            SetLights(true, false, false);
            yield return new WaitForSeconds(redDuration);
        }
    }

    private void SetLights(bool red, bool yellow, bool green)
    {
        SetBulb(redMaterial, Color.red, red);
        SetBulb(yellowMaterial, Color.yellow, yellow);
        SetBulb(greenMaterial, Color.green, green);
    }

    private void SetBulb(Material material, Color color, bool enabled)
    {
        material.SetColor(
            EmissionColor,
            enabled ? color * 3f : Color.black
        );
    }
}
