using UnityEngine;
using UnityEngine.InputSystem;

public class MarkEnemy : MonoBehaviour, ISkill
{
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private float markDuration = 10f;

    private Camera mainCamera;

    private void Awake() => mainCamera = Camera.main;

    public SkillTypes SkillType() => SkillTypes.MarkEnemy;
    public SkillActivationMode ActivationMode() => SkillActivationMode.Raycast;
    public float GetCooldown() => 4f;

    public void TryActivate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 500f, enemyMask))
        {
            var enemy = hit.collider.GetComponent<IEnemy>();
            if (enemy != null) Mark(enemy);
        }
    }

    private void Mark(IEnemy enemy)
    {
        var marker = Instantiate(markerPrefab, enemy.Transform.position, Quaternion.identity, enemy.Transform);
        Destroy(marker, markDuration);
        // TODO set enemy as marked
    }
}
