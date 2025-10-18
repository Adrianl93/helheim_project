using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public class Mainmenu : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject mainMenu;

    private PlayerInput playerInput;

    private void Awake()
    {
       
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            //imput de tecla ESC
            playerInput.actions["Exit"].performed += ctx => QuitGame();
        }
        else
        {
            Debug.LogWarning("[MainMenu] No se encontró PlayerInput en el objeto. Agregá un PlayerInput con tu Input Actions Asset.");
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

    public void QuitGame()
    {
        //IMPORTANTE NO TOCAR: este IF raro funciona para que lo detecte unity editor y no tire error al ejecutar Application.Quit en el editor ya que esta funcion no se permite en el editor
        Debug.Log("[MainMenu] Cerrando el juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Scene1");
    }
}
