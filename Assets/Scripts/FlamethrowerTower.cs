using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class FlamethrowerTower : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 20f;
    [SerializeField] private float range = 10f;
    [SerializeField] private float flameAngle = 60f;

    [SerializeField] MeshCollider flameCollider;
    [SerializeField] private Transform pivot;

    private readonly Dictionary<int, Enemy> enemiesInRange = new();
    private Enemy currentTarget;

    private void OnDrawGizmosSelected()
    {
        float radius = range;
        float angle = flameAngle;

        Handles.color = Color.cyan;
        Vector3 position = transform.position;

        Handles.DrawWireArc(
            position,
            Vector3.up,
            Quaternion.Euler(0, -angle / 2f, 0) * transform.forward,
            angle,
            radius
        );

        Handles.DrawLine(position, position + transform.forward * radius);
    }

    void Start()
    {
        flameCollider = GetComponent<MeshCollider>();
        flameCollider.transform.localScale = new Vector3(range, range, range);
    }

    void Update()
    {
        
    }
}
