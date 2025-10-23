using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSelectable : MonoBehaviour
{
    [Header("Visual References")]
    [SerializeField] private GameObject highlightIndicator;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private GameObject rangeIndicator;

    public bool IsSelected { get; private set; }
    private bool isHovered;

    private void Awake()
    {
        highlightIndicator.SetActive(false);
        rangeIndicator.SetActive(false);
    }

    public void OnHoverEnter()
    {
        if (IsSelected) return;
        ApplyMaterial(hoverMaterial);
        highlightIndicator.SetActive(true);
        isHovered = true;
    }

    public void OnHoverExit()
    {
        if (IsSelected) return;
        highlightIndicator.SetActive(false);
        isHovered = false;
    }

    public void Select()
    {
        IsSelected = true;
        ApplyMaterial(selectedMaterial);
        highlightIndicator.SetActive(true);
        rangeIndicator.SetActive(true);
    }

    public void Deselect()
    {
        IsSelected = false;
        rangeIndicator.SetActive(false);
        if (isHovered)
        {
            ApplyMaterial(hoverMaterial);
            highlightIndicator.SetActive(true);
        }
        else
        {
            highlightIndicator.SetActive(false);
        }
    }

    private void ApplyMaterial(Material materialToApply)
    {
        var renderers = highlightIndicator.GetComponentsInChildren<Renderer>(includeInactive: true);
        foreach (var  renderer in renderers)
        {
            if (renderer == null) continue;

            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = materialToApply;
            }
            renderer.sharedMaterials = materials;
        }
    }
}
