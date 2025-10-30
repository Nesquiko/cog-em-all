using TMPro;
using UnityEngine;

public class WaveCounterInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;

    public void SetCounter(int current, int total)
    {
        counterText.text = $"{current}/{total}";
    }
}
