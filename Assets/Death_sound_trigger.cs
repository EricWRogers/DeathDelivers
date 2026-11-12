using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SquishySound : MonoBehaviour
{
    [SerializeField] private AudioClip squishSound;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    private void OnEnable()
    {
        UnityEngine.Debug.Log(
            "NPC sound script enabled: " + gameObject.name);

        if (squishSound != null && Application.isPlaying)
        {
            float masterVol = PlayerPrefs.GetFloat("masterVolume", 1f);
            float finalVolume = volume * masterVol;

            AudioSource.PlayClipAtPoint(
                squishSound,
                transform.position,
                finalVolume
            );
        }
    }
}
