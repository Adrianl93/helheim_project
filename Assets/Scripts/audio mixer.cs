using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerSlider : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string exposedParameter = "MasterVolume";

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;

    private void Start()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);

            float savedVolume = PlayerPrefs.GetFloat(exposedParameter, 0.75f);
            volumeSlider.value = savedVolume;
            SetVolume(savedVolume);
        }
    }

    public void SetVolume(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.001f, 1f)) * 20f;
        audioMixer.SetFloat(exposedParameter, dB);

        PlayerPrefs.SetFloat(exposedParameter, volume);
    }
}
