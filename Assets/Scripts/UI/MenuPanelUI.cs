using UnityEngine;
using UnityEngine.UI;

public class MenuPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image nexusHealthFill;
    [SerializeField] private Image experienceFill;
    [SerializeField] private float lerpTime = 5f;

    private float currentNexusHealthFill;
    private float targetNexusHealthFill;

    private float currentExperienceFill;
    private float targetExperienceFill;

    private bool nexusHealthInitialized;
    private bool experienceInitialized;

    private void Update()
    {
        if (currentNexusHealthFill != targetNexusHealthFill)
        {
            currentNexusHealthFill = Mathf.Lerp(currentNexusHealthFill, targetNexusHealthFill, Time.deltaTime * lerpTime);
            nexusHealthFill.fillAmount = currentNexusHealthFill;
        }

        if (currentExperienceFill != targetExperienceFill)
        {
            currentExperienceFill = Mathf.Lerp(currentExperienceFill, targetExperienceFill, Time.deltaTime * lerpTime);
            experienceFill.fillAmount = currentExperienceFill;
        }
    }

    public void UpdateNexusHealth(float normalizedHealth)
    {
        normalizedHealth = Mathf.Clamp01(normalizedHealth);

        if (!nexusHealthInitialized)
        {
            currentNexusHealthFill = normalizedHealth;
            nexusHealthInitialized = true;
        }

        targetNexusHealthFill = normalizedHealth;
    }

    public void UpdateExperience(float normalizedExperience)
    {
        normalizedExperience = Mathf.Clamp01(normalizedExperience);

        if (!experienceInitialized)
        {
            currentExperienceFill = normalizedExperience;
            experienceInitialized = true;
        }

        targetExperienceFill = normalizedExperience;
    }
}
