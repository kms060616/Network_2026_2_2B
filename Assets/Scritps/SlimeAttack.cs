using UnityEngine;



[RequireComponent(typeof(SlimeUnit))]
public class SlimeAttack : MonoBehaviour
{
    private SlimeUnit unit;
    private EnemyHealth target;
    private float cooldown;

    private void Awake()
    {
        unit = GetComponent<SlimeUnit>();
    }

    private void Update()
    {
        SlimeData data = unit.Data;

        if (data == null)
            return;

        cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

        if (!IsValidTarget(target, data.attackRange))
            target = FindNearestEnemy(data.attackRange);

        if (target == null || cooldown > 0f)
            return;

        target.TakeDamage(data.attackDamage);
        cooldown = Mathf.Max(0.01f, data.attackInterval);
    }

    private bool IsValidTarget(EnemyHealth enemy, float range)
    {
        if (enemy == null || enemy.IsDead ||
            !enemy.isActiveAndEnabled)
        {
            return false;
        }

        Vector2 offset =
            enemy.transform.position - transform.position;

        return offset.sqrMagnitude <= range * range;
    }

    private EnemyHealth FindNearestEnemy(float range)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        EnemyHealth nearest = null;
        float nearestDistanceSquared = range * range;

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsValidTarget(enemy, range))
                continue;

            Vector2 offset =
                enemy.transform.position - transform.position;

            float distanceSquared = offset.sqrMagnitude;

            if (nearest == null ||
                distanceSquared < nearestDistanceSquared)
            {
                nearest = enemy;
                nearestDistanceSquared = distanceSquared;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        SlimeUnit slime = GetComponent<SlimeUnit>();

        if (slime == null || slime.Data == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            slime.Data.attackRange
        );
    }
}
