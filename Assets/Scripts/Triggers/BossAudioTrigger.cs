using UnityEngine;
using System.Collections;

public class BossAudioTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource bossMusicSource;
    [SerializeField] private AudioSource bossVoiceSource; // ✅ NUEVO
    public GameObject bossObject;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 2.5f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float delayAfterVoice = 1f; // ✅ Tiempo entre voz y música

    private IBossState bossState;
    private bool soundPlayed = false;
    private bool fadeOutStarted = false;

    private void Awake()
    {
        if (bossObject != null)
        {
            bossState = bossObject.GetComponent<IBossState>();

            if (bossState == null)
                Debug.LogWarning("[BossAudioTrigger] ¡El boss no implementa IBossState!");
        }
    }

    private void Update()
    {
        if (bossState != null && bossState.IsDead)
        {
            if (!fadeOutStarted)
            {
                fadeOutStarted = true;
                StartCoroutine(FadeOutMusic());
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || bossState == null)
            return;

        if (!bossState.IsDead && !soundPlayed)
        {
            soundPlayed = true;
            StartCoroutine(PlayBossVoiceBeforeMusic());
            Debug.Log("[BossAudioTrigger] ¡Voz del boss + música activada!");
        }
    }

    private IEnumerator PlayBossVoiceBeforeMusic()
    {
        if (bossVoiceSource != null)
        {
            bossVoiceSource.Play();
            yield return new WaitForSeconds(bossVoiceSource.clip.length + delayAfterVoice);
        }

        StartCoroutine(FadeInMusic());
    }

    private IEnumerator FadeInMusic()
    {
        bossMusicSource.volume = 0f;
        bossMusicSource.Play();

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            bossMusicSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / fadeInDuration);
            yield return null;
        }

        bossMusicSource.volume = maxVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float initialVolume = bossMusicSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            bossMusicSource.volume = Mathf.Lerp(initialVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        bossMusicSource.Stop();
        bossMusicSource.volume = 0f;

        Destroy(gameObject);
    }
}