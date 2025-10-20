using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject mainMenu;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            playerInput.actions["Exit"].performed += ctx => QuitGame();
        }
        else
        {
            Debug.LogWarning("[MainMenu] Falta componente PlayerInput.");
        }
    }

    public void OpenOptionsPanel()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void OpenMainMenuPanel()
    {
        mainMenu.SetActive(true);
        optionsMenu.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Scene1");
        Debug.Log("[MainMenu] Cargando Scene1...");
    }

    public void QuitGame()
    {

        //IMPORTANTE NO TOCAR: este IF raro funciona para que lo detecte unity editor y no tire error al ejecutar Application.Quit en el editor ya que esta funcion no se permite en el editor
        Debug.Log("[MainMenu] Cerrando juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
