using UnityEngine;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pauseMessageUI;

    private PauseManager pauseManager;

    private void Awake()
    {
        pauseManager = FindFirstObjectByType<PauseManager>();
    }

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

    // Métodos conectados a los botones del menú de pausa
    public void OnResumeButton() => pauseManager?.ResumeFromButton();
    public void OnExitButton() => pauseManager?.ExitToMenu();
}
