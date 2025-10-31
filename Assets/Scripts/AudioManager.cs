using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [Header("Audio")]
    [SerializeField] private AudioClip ambientMusic;
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        //singleton para evitar duplicaciones
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = ambientMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = musicVolume;

        if (ambientMusic != null)
            audioSource.Play();

        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene scene)
    {
        // nos aseguramos de que se destruya (estuvimos teniendo problemas con audio listener duplicados)
        Destroy(gameObject);
    }

    #region Volumen
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
            audioSource.volume = musicVolume;
    }

    public void IncreaseVolume(float increment = 0.05f) => SetMusicVolume(musicVolume + increment);
    public void DecreaseVolume(float decrement = 0.05f) => SetMusicVolume(musicVolume - decrement);
    #endregion
}
