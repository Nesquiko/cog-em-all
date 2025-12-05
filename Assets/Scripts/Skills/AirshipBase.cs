using System.Collections;
using UnityEngine;

public abstract class AirshipBase : MonoBehaviour, ISkill
{
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private float approachHeight = 80f;
    [SerializeField] private float approachSpeed = 50f;
    [SerializeField] private float descentDuration = 3f;

    public abstract SkillTypes SkillType();
    public abstract float GetCooldown();
    public SkillActivationMode ActivationMode() => SkillActivationMode.Airship;

    public void Trigger(Vector3 targetPosition)
    {
        StartCoroutine(DropFromAirship(targetPosition));
    }

    private IEnumerator DropFromAirship(Vector3 target)
    {
        Vector3 startPosition = target + Vector3.up * approachHeight;
        GameObject skill = Instantiate(skillPrefab, startPosition, Quaternion.identity);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / descentDuration;
            skill.transform.position = Vector3.Lerp(startPosition, target, t);
            yield return null;
        }

        OnAirshipSkillArrived(skill, target);
    }

    protected abstract void OnAirshipSkillArrived(GameObject airship, Vector3 target);
}
