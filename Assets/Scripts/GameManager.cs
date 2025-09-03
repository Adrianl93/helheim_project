using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
       
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        CloseGame();
        RebootGame();
    }

    // Cerrar el juego con Escape
    public void CloseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // Reiniciar el juego manualmente con R
    public void RebootGame()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("Scene1");
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Scene1");
    }
}
