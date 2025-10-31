using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private TextMeshProUGUI confirmationButtonText;
    [SerializeField] private Button confirmButton;

    private Action onConfirm;

    public void Initialize(string confirmText, string confirmButtonText, Action onConfirmAction)
    {
        confirmationText.text = confirmText;
        confirmationButtonText.text = confirmButtonText;
        onConfirm = onConfirmAction;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmPressed);

        gameObject.SetActive(true);
    }

    private void OnConfirmPressed()
    {
        onConfirm.Invoke();
        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
