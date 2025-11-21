using UnityEngine;

[CreateAssetMenu(fileName = "LeftoverDebrisModifier", menuName = "Skill Modifiers/Leftover Debris Modifier")]
public class LeftoverDebrisModifier : ScriptableObject, ISkillModifier
{
    [Header("Generic")]
    [SerializeField] private SkillTypes skillType = SkillTypes.Wall;
    [SerializeField] private SkillModifiers modifier = SkillModifiers.LeftoverDebris;
    [SerializeField] private string displayName = "Leftover Debris";
    [SerializeField, TextArea] private string description = "Leftover Debris description";

    [Header("Specific")]
    public float debrisAreaRange = 15f;
    public float debrisDuration = 5f;
    public EnemyStatusEffect debrisSlow = EnemyStatusEffect.DebrisSlow();
    public GameObject debrisAreaPrefab;

    public SkillTypes SkillType => skillType;
    public SkillModifiers Modifier => modifier;
    public string DisplayName => displayName;
    public string Description => description;
}
