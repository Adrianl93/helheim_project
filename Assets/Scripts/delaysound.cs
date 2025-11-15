using UnityEngine;
using System.Collections;

public class StartAudioDelayed : MonoBehaviour
{
    private AudioSource audioSource;
    // La variable startDelay ya no es necesaria, pero la dejamos por si quieres un retraso extra.

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            StartCoroutine(StartAudioAfterPaint());
        }
    }

    IEnumerator StartAudioAfterPaint()
    {
        // 1. Esperar un frame. Esto asegura que Unity ha tenido tiempo de dibujar
        // la interfaz de usuario y que el motor ha inicializado la escena.
        yield return null;

        // Opcional: Si el audio sigue sonando antes de tiempo, puedes añadir
        // yield return new WaitForEndOfFrame();
        // para esperar hasta después del ciclo de renderizado.

        audioSource.Play();
    }
}