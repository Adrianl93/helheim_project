using UnityEngine;

public class ExitGame : MonoBehaviour
{
    // Llama a este m�todo para cerrar el juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

        // Cierra la aplicaci�n
        Application.Quit();

        // Esto solo se ve en el editor (no en el build)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
