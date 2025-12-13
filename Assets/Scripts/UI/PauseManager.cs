using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject confirmation;
    [SerializeField] private Toggle damageDealtCheckbox;
    [SerializeField] private Toggle gearDropCheckbox;

    [SerializeField] private TowerSelectionManager towerSelectionManager;

    private SoundMixerManager soundMixerManager;

    private ConfirmationDialog confirmationDialog;

    private bool isPaused = false;

    public bool Paused => isPaused;

    private void Awake()
    {
        confirmationDialog = confirmation.GetComponent<ConfirmationDialog>();

        soundMixerManager = FindFirstObjectByType<SoundMixerManager>();

        damageDealtCheckbox.isOn = PlayerPrefs.GetInt("ShowDamageDealt") == 1;
        gearDropCheckbox.isOn = PlayerPrefs.GetInt("ShowGearDrops") == 1;

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
        PlayerPrefs.SetInt("ShowDamageDealt", value ? 1 : 0);
    }

    public void ToggleShowGearDrops(bool value)
    {
        PlayerPrefs.SetInt("ShowGearDrops", value ? 1 : 0);
    }

    public void SetMasterVolume(float level)
    {
        SoundMixerManager.Instance.SetMasterVolume(level);
    }

    public void SetSoundFXVolume(float level)
    {
        SoundMixerManager.Instance.SetSoundFXVolume(level);
    }

    public void SetMusicVolume(float level)
    {
        SoundMixerManager.Instance.SetMusicVolume(level);
    }
}
