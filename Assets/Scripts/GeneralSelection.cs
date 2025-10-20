using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GeneralSelection : MonoBehaviour
{
    [SerializeField] private RectTransform[] generals;
    [SerializeField] private RectTransform generalInfo;
    [SerializeField] private float moveDuration = 0.7f;
    [SerializeField] private Vector3 leftTarget = new(-400f, 0f, 0f);
    [SerializeField] private Vector3 backOffset = new(0f, 0f, -150f);

    private Vector3[] originalPositions;
    private bool selecting = false;

    private void Awake()
    {
        originalPositions = new Vector3[generals.Length];
        for (int i = 0; i < generals.Length; i++)
        {
            originalPositions[i] = generals[i].anchoredPosition;
        }

        generalInfo.gameObject.SetActive(true);
    }

    public void OnSelectGeneral(int index)
    {
        if (selecting) return;
        selecting = true;
        StartCoroutine(AnimateSelection(index));
    }

    private IEnumerator AnimateSelection(int selectedIndex)
    {
        float t = 0;
        generalInfo.gameObject.SetActive(true);
        CanvasGroup generalInfoCanvas = generalInfo.GetComponent<CanvasGroup>();
        if (generalInfoCanvas == null)
        {
            generalInfoCanvas = generalInfoCanvas.gameObject.AddComponent<CanvasGroup>();
            generalInfoCanvas.alpha = 0;
        }

        Vector3 startPositionSelection = generals[selectedIndex].anchoredPosition;
        Vector3 endPositionSelection = leftTarget;

        Vector3[] startPositionOthers = new Vector3[generals.Length];
        for (int i = 0; i < generals.Length; i++)
        {
            startPositionOthers[i] = generals[i].anchoredPosition;
        }

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0, 1, t / moveDuration);

            generals[selectedIndex].anchoredPosition3D = Vector3.Lerp(startPositionSelection, endPositionSelection, p);
        
            for (int i = 0; i < generals.Length; i++)
            {
                if (i == selectedIndex) continue;
                generals[i].anchoredPosition3D = Vector3.Lerp(
                    startPositionOthers[i],
                    originalPositions[i] + backOffset,
                    p
                );
            }

            generalInfoCanvas.alpha = Mathf.Lerp(0, 1, p);

            yield return null;
        }

        selecting = false;
    }

    public void ResetPositions()
    {
        StopAllCoroutines();
        for (int i = 0; i < generals.Length; i++)
        {
            generals[i].anchoredPosition3D = originalPositions[i];
        }
        generalInfo.gameObject.SetActive(false);
        selecting = false;
    }
}
