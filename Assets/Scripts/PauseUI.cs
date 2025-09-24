using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMessageUI; 

    private void OnEnable()
    {
        GameManager.OnGamePaused += ShowPauseMessage;
        GameManager.OnGameResumed += HidePauseMessage;
    }

    private void OnDisable()
    {
        GameManager.OnGamePaused -= ShowPauseMessage;
        GameManager.OnGameResumed -= HidePauseMessage;
    }

    private void ShowPauseMessage()
    {
        if (pauseMessageUI != null)
            pauseMessageUI.SetActive(true);
    }

    private void HidePauseMessage()
    {
        if (pauseMessageUI != null)
            pauseMessageUI.SetActive(false);
    }
}
