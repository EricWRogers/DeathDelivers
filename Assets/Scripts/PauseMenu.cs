using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject controlsPanel;
    public GameObject creditsPanel;

    private bool isPaused = false;

    void Update()
    {
        // Check for ESC key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If Controls or Credits is open,
            // ESC returns to the main Pause Menu
            if (controlsPanel.activeSelf || creditsPanel.activeSelf)
            {
                BackToPauseMenu();
            }
            // If the Pause Menu is already open,
            // ESC does nothing
            else if (isPaused)
            {
                return;
            }
            // If the game is running, ESC opens the Pause Menu
            else
            {
                Pause();
            }
        }
    }

    // Opens the Pause Menu and freezes the game
    public void Pause()
    {
        isPaused = true;

        pausePanel.SetActive(true);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    // Closes the Pause Menu and resumes the game
    public void Resume()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    // Opens the Controls panel
    public void ShowControls()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    // Opens the Credits panel
    public void ShowCredits()
    {
        pausePanel.SetActive(false);
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    // Returns from Controls or Credits to the Pause Menu
    public void BackToPauseMenu()
    {
        controlsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}