#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerPrefsResetOnPause
{
    static PlayerPrefsResetOnPause()
    {
        // Detectar pausa o reanudación
        EditorApplication.pauseStateChanged += OnPauseStateChanged;

        // Detectar cuando se detiene el Play Mode
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPauseStateChanged(PauseState state)
    {
        if (state == PauseState.Paused)
        {
            Debug.Log("[Editor] Juego pausado: se borrarán PlayerPrefs");
            PlayerPrefs.DeleteAll();
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            Debug.Log("[Editor] Play Mode detenido: se borrarán PlayerPrefs");
            PlayerPrefs.DeleteAll();
        }
    }
}
#endif
