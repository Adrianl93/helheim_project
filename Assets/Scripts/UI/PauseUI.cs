using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMessageUI;

    private void OnEnable()
    {
        PauseManager.OnGamePaused += ShowPauseMessage;
        PauseManager.OnGameResumed += HidePauseMessage;
    }

    private void OnDisable()
    {
        PauseManager.OnGamePaused -= ShowPauseMessage;
        PauseManager.OnGameResumed -= HidePauseMessage;
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

    // Métodos UI para botones
    public void TogglePause()
    {
        if (PauseManagerInstance != null)
            PauseManagerInstance.TogglePause();
    }

    public void PauseGame()
    {
        if (PauseManagerInstance != null)
            PauseManagerInstance.PauseGame();
    }

    public void ResumeGame()
    {
        if (PauseManagerInstance != null)
            PauseManagerInstance.ResumeGame();
    }

    private PauseManager PauseManagerInstance => _pauseManager != null ? _pauseManager : _pauseManager = Object.FindFirstObjectByType<PauseManager>();

    private PauseManager _pauseManager;
}
