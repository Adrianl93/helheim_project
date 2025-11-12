using UnityEngine;
using UnityEngine.SceneManagement;

public class History : MonoBehaviour
{
    [Tooltip("Segundos que espera antes de cargar la siguiente escena automáticamente")]
    public float delayBeforeLoading = 0f;

    [Tooltip("Nombre de la escena a cargar")]
    public string nextSceneName = "Menu 1";

    [Tooltip("¿Cargar automáticamente después del retraso?")]
    public bool autoLoad = true;

    void Start()
    {
        // Si está activado el autoLoad, programar la carga automática
        if (autoLoad)
            Invoke(nameof(LoadNextScene), delayBeforeLoading);
    }

    // Puede ser llamada automáticamente o desde un botón (OnClick)
    public void LoadNextScene()
    {
        Debug.Log("Botón presionado — cargando escena: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
    public void TestClick()
    {
        Debug.Log("El botón está vinculado correctamente ✅");
    }
}
