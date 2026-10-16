using UnityEngine;



[RequireComponent(typeof(SlimeUnit))]
public class SlimeAttack : MonoBehaviour
{
    [SerializeField] private bool logCriticalHits;
    private SlimeUnit unit;
    private EnemyHealth target;
    private float cooldown;
    private SlimeSkillRunner skillRunner;

    private void Awake()
    {
        unit = GetComponent<SlimeUnit>();
        skillRunner = GetComponent<SlimeSkillRunner>();
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

        float damage = unit.AttackDamage;
        bool critical = false;

        SynergyManager manager = SynergyManager.Instance;

        if (manager != null)
        {
            float chance = manager.GetCriticalChance(data.element);

            if (chance > 0f && Random.value < chance)
            {
                critical = true;
                damage *= manager.GetCriticalMultiplier();
            }
        }

        if (critical && logCriticalHits)
        {
            Debug.Log(
                $"{name} 치명타! 피해: {damage:0.##}",
                this
            );
        }
        Vector3 hitPosition = target.transform.position;

        target.TakeDamage(damage);
        cooldown = unit.AttackInterval;

        if (skillRunner != null)
            skillRunner.NotifyBasicAttack(hitPosition);

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
