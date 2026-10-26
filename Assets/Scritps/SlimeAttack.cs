using UnityEngine;



[RequireComponent(typeof(SlimeUnit))]
public class SlimeAttack : MonoBehaviour
{
    [SerializeField] private SlimeProjectile projectilePrefab;
    [SerializeField] private bool logCriticalHits;

    private SlimeUnit unit;
    private SlimeSkillRunner skillRunner;
    private SlimeIdleAnimation visualAnimation;

    private EnemyHealth target;
    private float cooldown;

    private void Awake()
    {
        unit = GetComponent<SlimeUnit>();
        skillRunner = GetComponent<SlimeSkillRunner>();

        visualAnimation =
            GetComponentInChildren<SlimeIdleAnimation>(true);
    }

    private void Start()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError(
                "SlimeAttack에 투사체 프리팹을 연결해주세요.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (unit.IsDispatched || Time.timeScale == 0f)
            return;

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

        Vector3 targetPosition = target.transform.position;

        SlimeProjectile projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        projectile.Initialize(
            target,
            damage,
            GetProjectileColor(data.element)
        );

        cooldown = unit.AttackInterval;

        if (visualAnimation != null)
            visualAnimation.PlayAttack(unit.AttackInterval);

        if (critical && logCriticalHits)
        {
            Debug.Log(
                $"{name} 치명타 발사! 예정 피해: {damage:0.##}",
                this
            );
        }

        // 공격 횟수·확률 스킬은 발사 시점을 기준으로 판정합니다.
        if (skillRunner != null)
        {
            skillRunner.NotifyBasicAttack(
                targetPosition,
                target
            );
        }
        skillRunner.NotifyBasicAttack(targetPosition);
    }

    private bool IsValidTarget(EnemyHealth enemy, float range)
    {
        if (enemy == null ||
            enemy.IsDead ||
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
        EnemyHealth nearest = null;
        float nearestDistanceSquared = range * range;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

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

    private static Color GetProjectileColor(SlimeElement element)
    {
        return element switch
        {
            SlimeElement.Water => new Color(0.3f, 0.7f, 1f),
            SlimeElement.Fire => new Color(1f, 0.45f, 0.15f),
            SlimeElement.Grass => new Color(0.3f, 1f, 0.4f),
            SlimeElement.Electric => new Color(1f, 0.9f, 0.2f),
            _ => Color.white
        };
    }
}
