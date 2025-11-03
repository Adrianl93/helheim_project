using UnityEngine;
using System.Collections;

public class BossAudioTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource bossAudioSource;
    [SerializeField] private GameObject bossObject;

    [Header("Sonidos del Boss")]
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip bossScream; // Voz o grito al activarse

    [Header("Configuración de Audio")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 2f;

    private IBossState bossState;
    private bool musicStarted = false;

    private void Awake()
    {
        if (bossObject != null)
        {
            bossState = bossObject.GetComponent<IBossState>();

            if (bossState == null)
                Debug.LogWarning("[BossAudioTrigger] El boss NO implementa IBossState!");
        }
    }

    private void Update()
    {
        if (bossState == null) return;

        if (bossState.IsDead)
        {
            StartCoroutine(FadeOutAndStop());
            Destroy(gameObject, fadeOutDuration + 0.2f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (bossState == null) return;
        if (bossState.IsDead) return;
        if (musicStarted) return;

        musicStarted = true;

        if (bossScream != null)
            AudioSource.PlayClipAtPoint(bossScream, transform.position);

        // ⏱ Nuevo: Espera para que el grito suene primero
        StartCoroutine(DelayedMusicStart());

        Debug.Log("[BossAudioTrigger] Música del Boss activada con delay!");
    }

    private IEnumerator DelayedMusicStart()
    {
        yield return new WaitForSeconds(1f); // Ajusta el tiempo del delay
        StartCoroutine(FadeInMusic());
    }
    private IEnumerator FadeInMusic()
    {
        bossAudioSource.clip = bossMusic;
        bossAudioSource.volume = 0f;
        bossAudioSource.Play();

        float t = 0f;

        while (t < fadeInDuration)
        {
            bossAudioSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            t += Time.deltaTime;
            yield return null;
        }

        bossAudioSource.volume = 1f;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = bossAudioSource.volume;
        float t = 0f;

        while (t < fadeOutDuration)
        {
            bossAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutDuration);
            t += Time.deltaTime;
            yield return null;
        }

        bossAudioSource.Stop();
        bossAudioSource.volume = 1f;
    }
}
