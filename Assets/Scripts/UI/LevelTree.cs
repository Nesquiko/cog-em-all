using UnityEngine;

public class LevelTree : MonoBehaviour
{
    [SerializeField] private GameObject[] currentLevelIndicators;

    private SaveContextDontDestroy saveContext;

    private void OnEnable()
    {
        UpdateCurrentLevel();        
    }

    private void UpdateCurrentLevel()
    {
        saveContext = SaveContextDontDestroy.GetOrCreateDev();

        int currentFactionLevel = saveContext.LastFactionSaveState().level;

        for (int i = 1; i <= currentLevelIndicators.Length; i++)
            currentLevelIndicators[i - 1].SetActive(i == currentFactionLevel);
    }
}
