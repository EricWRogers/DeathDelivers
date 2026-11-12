using UnityEngine;

public class Death_sound_trigger : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private AudioClip squishSound;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private void Awake()
    {
        if (squishSound == null)
        {
            UnityEngine.Debug.LogWarning(
                "Missing sound on " + gameObject.name
            );
            return;
        }

        
        AudioListener listener =
            FindFirstObjectByType<AudioListener>();

        if (listener == null)
        {
            UnityEngine.Debug.LogWarning(
                "No Audio Listener found!"
            );
            return;
        }

        float masterVol = PlayerPrefs.GetFloat("masterVolume", 1f);
        float finalVolume = volume * masterVol;

        AudioSource.PlayClipAtPoint(
            squishSound,
            listener.transform.position,
            finalVolume
        );

        UnityEngine.Debug.Log(
            "Blood sound played for: " + gameObject.name
        );
    }
}
