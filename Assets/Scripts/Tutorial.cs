using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tutorial : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject tutorialCanvas;
    public GameObject mainUICanvas;

    [Header("Stamp Scale Animation (Stays in Corner)")]
    public Vector3 enlargedScale = new Vector3(17.78f, 11.78f, 10.67f);
    public float animDuration = 0.5f;

    [Header("Tutorial Lists (Drag Disabled Objects in Order)")]
    public List<GameObject> textObjects = new List<GameObject>();
    public List<GameObject> portraitObjects = new List<GameObject>();

    private int currentIndex = 0;
    private int totalSteps = 0;
    private bool isTutorialActive = false;
    private bool isAnimating = false;

    private Vector3 smallStampScale;

    void Start()
    {
        StartTutorial();
    }

    void StartTutorial()
    {
        totalSteps = Mathf.Max(textObjects.Count, portraitObjects.Count);

        if (totalSteps == 0)
        {
            EndTutorial();
            return;
        }

        // 1. Hide ALL text and ALL portraits at the very start
        foreach (GameObject txt in textObjects)
        {
            if (txt != null) txt.SetActive(false);
        }

        foreach (GameObject portrait in portraitObjects)
        {
            if (portrait != null) portrait.SetActive(false);
        }

        currentIndex = 0;

        // Swap canvases and freeze the game
        tutorialCanvas.SetActive(true);
        mainUICanvas.SetActive(false);
        Time.timeScale = 0f;

        // Unlock cursor so the player can click
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(IntroStampSequence());
    }

    IEnumerator IntroStampSequence()
    {
        isAnimating = true;

        // 2. Grab ONLY the first portrait (Element 0 in Portrait Objects)
        GameObject firstPortrait = GetFirstObject(portraitObjects);

        if (firstPortrait != null)
        {
            RectTransform firstStampRect = firstPortrait.GetComponent<RectTransform>();

            // Record its small resting scale in the corner
            smallStampScale = firstStampRect.localScale;

            // Show ONLY the small stamp (all text is still hidden)
            firstPortrait.SetActive(true);

            // Wait one frame so scene-loading lag doesn't skip the animation
            yield return null;

            // 3. Enlarge the stamp smoothly in place
            yield return StartCoroutine(AnimateStampScale(firstStampRect, smallStampScale, enlargedScale, animDuration));
        }

        // 4. Now that the stamp is enlarged, turn on the first text (Element 0 in Text Objects)
        GameObject firstText = GetFirstObject(textObjects);
        if (firstText != null)
        {
            firstText.SetActive(true);
        }

        isAnimating = false;
        isTutorialActive = true;
    }

    void Update()
    {
        if (!isTutorialActive || isAnimating) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AdvanceTutorial();
        }
    }

    void AdvanceTutorial()
    {
        SetStepActive(currentIndex, false);
        currentIndex++;

        if (currentIndex < totalSteps)
        {
            SetStepActive(currentIndex, true);
        }
        else
        {
            StartCoroutine(OutroStampSequence());
        }
    }

    IEnumerator OutroStampSequence()
    {
        isTutorialActive = false;
        isAnimating = true;

        // Grab the last portrait in Portrait Objects
        GameObject lastPortrait = GetLastObject(portraitObjects);

        if (lastPortrait != null)
        {
            RectTransform lastStampRect = lastPortrait.GetComponent<RectTransform>();
            lastPortrait.SetActive(true);

            Vector3 startScale = lastStampRect.localScale;

            // Shrink back down to the small stamp scale in the corner
            yield return StartCoroutine(AnimateStampScale(lastStampRect, startScale, smallStampScale, animDuration));
        }

        isAnimating = false;
        EndTutorial();
    }

    IEnumerator AnimateStampScale(RectTransform target, Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        target.localScale = startScale;

        while (elapsed < duration)
        {
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            elapsed += dt;

            float progress = Mathf.Clamp01(elapsed / duration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            target.localScale = Vector3.Lerp(startScale, endScale, smoothProgress);

            yield return null;
        }

        target.localScale = endScale;
    }

    GameObject GetFirstObject(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null) return list[i];
        }
        return null;
    }

    GameObject GetLastObject(List<GameObject> list)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] != null) return list[i];
        }
        return null;
    }

    void SetStepActive(int index, bool isActive)
    {
        if (index < textObjects.Count && textObjects[index] != null)
        {
            textObjects[index].SetActive(isActive);
        }

        if (index < portraitObjects.Count && portraitObjects[index] != null)
        {
            portraitObjects[index].SetActive(isActive);
        }
    }

    void EndTutorial()
    {
        tutorialCanvas.SetActive(false);
        mainUICanvas.SetActive(true);
        Time.timeScale = 1f;
    }
}
