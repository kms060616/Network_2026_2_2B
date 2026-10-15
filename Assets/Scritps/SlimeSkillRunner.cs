using UnityEngine;


[RequireComponent(typeof(SlimeUnit))]
public class SlimeSkillRunner : MonoBehaviour
{
    [SerializeField] private bool logSkillUse;

    public float CurrentMana => mana;
    public float RemainingCooldown => cooldownRemaining;

    private SlimeUnit unit;
    private SlimeData previousData;
    private SlimeSkillData previousSkill;

    private float mana;
    private float cooldownRemaining;

    private void Awake()
    {
        unit = GetComponent<SlimeUnit>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        SlimeData data = unit.Data;
        SlimeSkillData skill = data != null ? data.skill : null;

        // 합성으로 종류나 스킬이 바뀌면 자원을 초기화합니다.
        if (data != previousData || skill != previousSkill)
        {
            previousData = data;
            previousSkill = skill;
            mana = 0f;
            cooldownRemaining = 0f;
        }

        if (data == null || skill == null ||
            data.grade != SlimeGrade.Legendary ||
            unit.CurrentCell == null)
        {
            return;
        }

        float speedMultiplier = SynergyManager.Instance != null
            ? SynergyManager.Instance.GetSkillSpeedMultiplier(
                data.element
            )
            : 1f;

        float cost = Mathf.Max(1f, skill.manaCost);

        mana = Mathf.Min(
            cost,
            mana + skill.manaRecovery *
                speedMultiplier * Time.deltaTime
        );

        cooldownRemaining = Mathf.Max(
            0f,
            cooldownRemaining - speedMultiplier * Time.deltaTime
        );

        if (mana < cost || cooldownRemaining > 0f)
            return;

        EnemyHealth target = FindNearestEnemy(data.attackRange);

        // 적이 없으면 마나를 보관합니다.
        if (target == null)
            return;

        Vector3 center = target.transform.position;

        mana -= cost;
        cooldownRemaining = Mathf.Max(0f, skill.cooldown);

        CastSkill(skill, center);
    }

    private EnemyHealth FindNearestEnemy(float range)
    {
        EnemyHealth nearest = null;
        float bestDistance = range * range;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy.IsDead || !enemy.isActiveAndEnabled)
                continue;

            Vector2 offset =
                enemy.transform.position - transform.position;

            float distance = offset.sqrMagnitude;

            if (distance <= bestDistance)
            {
                nearest = enemy;
                bestDistance = distance;
            }
        }

        return nearest;
    }

    private void CastSkill(SlimeSkillData skill, Vector3 center)
    {
        float damage = unit.AttackDamage * skill.damageMultiplier;
        float radiusSquared = skill.effectRadius * skill.effectRadius;
        int hitCount = 0;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy.IsDead || !enemy.isActiveAndEnabled)
                continue;

            Vector2 offset = enemy.transform.position - center;

            if (offset.sqrMagnitude > radiusSquared)
                continue;

            enemy.TakeDamage(damage);
            hitCount++;
        }

        if (logSkillUse)
        {
            Debug.Log(
                $"{name}: {skill.skillName} 발동! " +
                $"피해 {damage:0.##}, 대상 {hitCount}마리",
                this
            );
        }
    }

    [ContextMenu("테스트: 마나와 쿨타임")]
    private void LogResources()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"마나: {mana:0.##}, " +
            $"남은 쿨타임: {cooldownRemaining:0.##}초",
            this
        );
    }
}
