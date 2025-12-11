using TMPro;
using UnityEngine;

public class WaveCounterInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text speedText;

    public void SetCounter(int current, int total)
    {
        counterText.text = $"{current}/{total}";
    }

    public void SetGameSpeed(float speed)
    {
        speedText.text = $"{speed}x";
    }
}
