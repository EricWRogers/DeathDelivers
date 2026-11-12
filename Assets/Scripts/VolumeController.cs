using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer myAudioMixer;
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        // TEMPORARY LINE: Deletes only the old volume setting
        PlayerPrefs.DeleteKey("masterVolume");

        if (PlayerPrefs.HasKey("masterVolume"))
        {
            LoadVolume();
        }
        else
        {
            volumeSlider.value = 1f;
            SetMasterVolume();
        }
    }

    public void SetMasterVolume()
    {
        float sliderValue = volumeSlider.value;
        myAudioMixer.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat("masterVolume", sliderValue);
    }

    private void LoadVolume()
    {
        float savedVolume = Mathf.Clamp(PlayerPrefs.GetFloat("masterVolume"), 0.0001f, 1f);
        volumeSlider.value = savedVolume;
        SetMasterVolume();
    }
}
