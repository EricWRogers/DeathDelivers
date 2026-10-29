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
                AudioSource.PlayClipAtPoint(squishSound,
                transform.position,
                volume
                );
            }
        }
}

