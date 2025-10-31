using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image progressBarFill;

    private float currentFill;

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        currentFill = Mathf.Lerp(currentFill, progress, Time.deltaTime * 5f);
        progressBarFill.fillAmount = currentFill;
    }
}
