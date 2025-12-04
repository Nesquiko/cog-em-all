using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SavedGameButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void SetLabel(string value)
    {
        label.text = value;
    }

    public void SetButtonOnClick(Action onClick)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick.Invoke());
    }
}
