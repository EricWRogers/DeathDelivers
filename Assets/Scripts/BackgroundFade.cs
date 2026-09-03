using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BackgroundFade : MonoBehaviour
{
    public Image[] backgroundImages;

    public float displayTime = 4f;
    public float fadeTime = 1f;

    private int currentImage = 0;

    void Start()
    {
        // Make the first image visible
        for (int i = 0; i < backgroundImages.Length; i++)
        {
            Color color = backgroundImages[i].color;
            color.a = (i == 0) ? 1f : 0f;
            backgroundImages[i].color = color;
        }

        StartCoroutine(FadeImages());
    }

    IEnumerator FadeImages()
    {
        while (true)
        {
            yield return new WaitForSeconds(displayTime);

            int nextImage = (currentImage + 1) % backgroundImages.Length;

            yield return StartCoroutine(CrossFade(
                backgroundImages[currentImage],
                backgroundImages[nextImage]
            ));

            currentImage = nextImage;
        }
    }

    IEnumerator CrossFade(Image current, Image next)
    {
        float time = 0f;

        Color currentColor = current.color;
        Color nextColor = next.color;

        while (time < fadeTime)
        {
            time += Time.deltaTime;

            float t = time / fadeTime;

            currentColor.a = Mathf.Lerp(1f, 0f, t);
            nextColor.a = Mathf.Lerp(0f, 1f, t);

            current.color = currentColor;
            next.color = nextColor;

            yield return null;
        }

        currentColor.a = 0f;
        nextColor.a = 1f;

        current.color = currentColor;
        next.color = nextColor;
    }
}