using UnityEngine;

public class SlimeAttack : MonoBehaviour
{
    [SerializeField, Min(0f)] private float attackRange = 3f;
    [SerializeField, Min(0f)] private float attackDamage = 10f;
    [SerializeField, Min(0.01f)] private float attackInterval = 1f;

    private EnemyHealth target;
    private float cooldown;

    private void Update()
    {
        cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);

        // 현재 적이 죽거나 사거리 밖으로 나가면 다시 선택합니다.
        if (!IsValidTarget(target))
            target = FindNearestEnemy();

        if (target == null || cooldown > 0f)
            return;

        target.TakeDamage(attackDamage);
        cooldown = Mathf.Max(0.01f, attackInterval);
    }

    private bool IsValidTarget(EnemyHealth enemy)
    {
        if (enemy == null || enemy.IsDead ||
            !enemy.isActiveAndEnabled)
        {
            return false;
        }

        Vector2 offset = enemy.transform.position - transform.position;
        return offset.sqrMagnitude <= attackRange * attackRange;
    }

    private EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        EnemyHealth nearest = null;
        float nearestDistanceSquared = attackRange * attackRange;

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsValidTarget(enemy))
                continue;

            Vector2 offset = enemy.transform.position - transform.position;
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

    // Scene 화면에서 선택한 슬라임의 사거리를 표시합니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
