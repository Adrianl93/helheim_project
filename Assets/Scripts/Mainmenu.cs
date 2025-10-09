using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    public GameObject optionsMenu;
    public GameObject mainMenu;

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
     Application.Quit();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Scene1");
    }
 
    
}

    
     
