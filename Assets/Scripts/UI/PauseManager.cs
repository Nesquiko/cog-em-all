using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject confirmation;

    [Header("References")]
    [SerializeField] private TowerSelectionManager towerSelectionManager;

    private ConfirmationDialog confirmationDialog;

    private bool isPaused = false;

    public bool Paused => isPaused;

    private void Awake()
    {
        confirmationDialog = confirmation.GetComponent<ConfirmationDialog>();
    }

    private void Start()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        confirmation.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (optionsMenu.activeSelf || confirmation.activeSelf)
            {
                ShowPauseMenu();
                return;
            }

            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);

        towerSelectionManager.DeselectCurrent();
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        confirmation.SetActive(false);
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    public void ShowOptionsMenu()
    {
        pauseMenu.SetActive(false);
        confirmation.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void ShowPauseMenu()
    {
        optionsMenu.SetActive(false);
        confirmation.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void RestartOperation()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        confirmationDialog.Initialize(
            "Are you sure? All progress will be lost.",
            "Restart",
            () => {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        );
        confirmation.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    public void QuitGame()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        confirmationDialog.Initialize(
            "Are you sure? All progress will be lost.",
            "Quit Game",
            () =>
            {
                Time.timeScale = 1f;
                Application.Quit();
            }
        );
        confirmation.SetActive(true);
    }

    public void ToggleShowDamageDealt(bool value)
    {
        Debug.Log($"Toggling damage dealt to: {value}");
    }
}
