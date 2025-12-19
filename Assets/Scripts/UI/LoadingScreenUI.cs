using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image progressBarFill;

    [Header("Cinematic Pictures")]
    [SerializeField] private Image pictureImage;
    [SerializeField] private Sprite[] pictures;

    [Header("Tips")]
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private LoadingTipsData loadingTipsData;

    private float currentFill;
    private int currentTipIndex = -1;

    private void OnEnable()
    {
        Assert.IsTrue(loadingTipsData.TipCount > 0);
        Assert.IsTrue(pictures.Length > 0);
        ShowCinematicPicture();
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

    private void ShowCinematicPicture()
    {
        int pictureIndex = Random.Range(0, pictures.Length);
        pictureImage.sprite = pictures[pictureIndex];
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
