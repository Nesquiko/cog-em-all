using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class OperationResultUI : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private TMP_Text title;

    [Header("Operation Info")]
    [SerializeField] private TMP_Text operationName;
    [SerializeField] private TMP_Text duration;
    [SerializeField] private TMP_Text waves;

    [Header("Offensive Performance")]
    [SerializeField] private TMP_Text enemies;
    [SerializeField] private TMP_Text damage;

    [Header("Resource Summary")]
    [SerializeField] private TMP_Text gears;
    [SerializeField] private TMP_Text towers;

    [Header("Tower Kills")]
    [SerializeField] private TMP_Text[] towerKills;
    [SerializeField] private CanvasGroup[] towerMVPs;

    [Header("Performance Rating")]
    [SerializeField] private TMP_Text performanceRating;
    [SerializeField] private Image[] performanceRatingImages;

    [Header("Performance Calculation Settings")]
    [SerializeField, Range(0f, 1f), Tooltip("Factors should sum up to 1")] private float wavesFactor = 0.5f;
    [SerializeField, Range(0f, 1f), Tooltip("Factors should sum up to 1")] private float killFactor = 0.25f;
    [SerializeField, Range(0f, 1f), Tooltip("Factors should sum up to 1")] private float damageFactor = 0.15f;
    [SerializeField, Range(0f, 1f), Tooltip("Factors should sum up to 1")] private float economyFactor = 0.10f;

    [Header("Rewards")]
    [SerializeField] private TMP_Text xpRewardText;

    [SerializeField] private CanvasGroup retryCanvasGroup;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private TowerDataCatalog towerDataCatalog;

    private OperationStatistics operationStatistics;

    private void Awake()
    {
        mainCamera = Camera.main;
        brain = mainCamera.GetComponent<CinemachineBrain>();
    }

    private void SetTitle(bool cleared)
    {
        title.text = cleared ? "OPERATION CLEARED" : "OPERATION FAILED";

        if (ColorUtility.TryParseHtmlString(cleared ? "#88FFC1" : "#C00011", out Color color))
            title.color = color;
        else
            title.color = Color.black;
    }

    private string FormatDuration(float seconds) =>
        TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss");

    private void ShowTowerKills(int[] towerKillsValues)
    {
        for (int i = 0; i < towerKills.Length; i++)
        {
            TowerData<TowerDataBase> towerData = towerDataCatalog.FromIndex(i);
            towerKills[i].text = $"{towerData.DisplayName}:   {towerKillsValues[i]}";
        }
    }

    private void ShowTowerMVP(int[] towerKillsValues)
    {
        int maxKills = towerKillsValues.Max();

        if (maxKills == 0 && towerKillsValues.All(kills => kills == 0))
        {
            foreach (var mvpCanvasGroup in towerMVPs)
            {
                mvpCanvasGroup.alpha = 0f;
            }
            return;
        }

        List<int> mvpIndices = new();
        for (int i = 0; i < towerKillsValues.Length; i++)
        {
            if (towerKillsValues[i] == maxKills)
            {
                mvpIndices.Add(i);
            }
        }

        for (int i = 0; i < towerMVPs.Length; i++)
        {
            if (i < towerKillsValues.Length)
            {
                if (mvpIndices.Contains(i))
                {
                    towerMVPs[i].alpha = 1f;
                    towerKills[i].text = $"<u>{towerKills[i].text}</u>";
                }
                else
                {
                    towerMVPs[i].alpha = 0f;
                }
            }
            else if (towerMVPs[i] != null)
            {
                towerMVPs[i].alpha = 0f;
            }
        }
    }

    private float CalculatePerformance(OperationStatistics statistics)
    {
        float wavesRatio = statistics.clearedWaves / statistics.totalWaves;
        float killRatio = statistics.killedEnemies / statistics.totalEnemies;
        float damageDealt = statistics.damageDealt;
        float damageTaken = statistics.damageTaken;
        float economyRatio = statistics.gearsSpent / statistics.gearsEarned;

        float performance =
            (wavesFactor * wavesRatio) +
            (killFactor * killRatio) +
            (damageFactor * (damageDealt / (damageDealt + damageTaken + 1))) +
            (economyFactor * (1f - economyRatio));

        return Mathf.Clamp01(performance);
    }

    private void ShowPerformance(float performance)
    {
        performanceRating.text = $"{(int)(performance * 100)} %";

        int starCount = performanceRatingImages.Length;
        float segment = 1f / starCount;
        for (int i = 0; i < starCount; i++)
        {
            float fill = Mathf.Clamp01((performance - (i * segment)) / segment);
            performanceRatingImages[i].fillAmount = fill;
        }
    }

    public void Initialize(OperationStatistics statistics)
    {
        SoundFXType sfxType = statistics.cleared ? SoundFXType.OperationCleared : SoundFXType.OperationFailed;
        SoundManagersDontDestroy.GerOrCreate().SoundFX.PlaySoundFXClip(sfxType, transform);

        operationStatistics = statistics;

        SetTitle(statistics.cleared);
        operationName.text = statistics.operationName;
        duration.text = FormatDuration(statistics.duration);
        waves.text = $"{statistics.clearedWaves} / {statistics.totalWaves}";
        enemies.text = $"{statistics.killedEnemies} / {statistics.totalEnemies}";
        damage.text = $"{statistics.damageDealt} / {statistics.damageTaken}";
        gears.text = $"{statistics.gearsEarned} / {statistics.gearsSpent}";
        towers.text = $"{statistics.towersBuilt} / {statistics.towersUpgraded}";

        retryCanvasGroup.alpha = statistics.cleared ? 0f : 1f;

        ShowTowerKills(statistics.towerKills);
        ShowTowerMVP(statistics.towerKills);

        xpRewardText.text = statistics.cleared ? $"{statistics.xpReward} XP Gained" : "";
        // ShowPerformance(CalculatePerformance(statistics));
    }

    public void RetryOperation()
    {
        if (operationStatistics.cleared) return;
        Time.timeScale = 1f;
        brain.enabled = true;
        //SceneLoader.ReloadCurrentScene();
        SceneTransition.GetOrCreate().ReloadCurrentScene();
    }

    public void Continue()
    {
        Time.timeScale = 1f;
        brain.enabled = true;
        //SceneLoader.LoadScene("MenuScene");
        SceneTransition.GetOrCreate().TransitionToScene("MenuScene");
        SoundManagersDontDestroy.GerOrCreate().Music.PlayMenuMusic();
    }
}
