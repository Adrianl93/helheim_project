using UnityEngine;
using UnityEngine.SceneManagement;

public class Instructions : MonoBehaviour
{
    public float delayBeforeLoading = 5f; // segundos de duraci�n
    public string nextSceneName = "MainMenu";

    void Start()
    {
        Invoke("LoadNextScene", delayBeforeLoading);
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene("Menu 1");
    }
}