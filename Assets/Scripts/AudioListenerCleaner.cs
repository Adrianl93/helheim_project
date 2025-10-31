using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioListenerCleaner : MonoBehaviour
{
    void Awake()
    {
        // Destruye cualquier otro AudioListener que exista
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length > 1)
        {
            for (int i = 0; i < listeners.Length - 1; i++)
            {
                Destroy(listeners[i]);
            }
        }
    }
}
