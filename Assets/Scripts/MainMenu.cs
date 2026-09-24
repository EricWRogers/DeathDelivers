using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("EthanTest");
    }

    public void Controls()
    {
        SceneManager.LoadScene("ControlsMenu");
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }
}