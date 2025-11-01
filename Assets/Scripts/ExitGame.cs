using UnityEngine;

public class ExitGame : MonoBehaviour
{
    // Llama a este método para cerrar el juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");

        // Cierra la aplicación
        Application.Quit();

        // Esto solo se ve en el editor (no en el build)
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
