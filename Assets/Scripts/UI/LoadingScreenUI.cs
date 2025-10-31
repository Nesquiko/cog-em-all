using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Tips")]
    [SerializeField] private LoadingTipsData loadingTipsData;

    private float currentFill;

    private int currentTipIndex = -1;

    private void OnEnable()
    {
        Assert.IsTrue(loadingTipsData.TipCount > 0);
        StartCoroutine(RotateTips());
    }

    private void Start()
    {
        StartCoroutine(SceneLoader.LoadTargetScene(UpdateProgress));
    }

    private void UpdateProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        currentFill = Mathf.Lerp(currentFill, progress, Time.deltaTime * 5f);
        progressBarFill.fillAmount = currentFill;
    }

    private IEnumerator RotateTips()
    {
        while (true)
        {
            ShowNextTip();
            yield return new WaitForSeconds(loadingTipsData.TipChangeInterval);
        }
    }

    private void ShowNextTip()
    {
        int nextIndex = Random.Range(0, loadingTipsData.TipCount);
        if (nextIndex == currentTipIndex)
            nextIndex = (nextIndex + 1) % loadingTipsData.TipCount;

        currentTipIndex = nextIndex;
        tipText.text = loadingTipsData.Tips[currentTipIndex];
    }
}
