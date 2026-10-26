using UnityEngine;


[RequireComponent(typeof(SlimeUnit))]
public class SlimeSkillRunner : MonoBehaviour
{
    [SerializeField] private bool logSkillUse;

    public float CurrentMana => mana;
    public float RemainingCooldown => cooldownRemaining;
    public int AttackCount => attackCount;

    private SlimeUnit unit;
    private SlimeData previousData;
    private SlimeSkillData previousSkill;

    private float mana;
    private float cooldownRemaining;
    private int attackCount;

    private void Awake()
    {
        unit = GetComponent<SlimeUnit>();
    }

    public void ResetState()
    {
        previousData = unit.Data;
        previousSkill = unit.Data != null
            ? unit.Data.skill
            : null;

        mana = 0f;
        cooldownRemaining = 0f;
        attackCount = 0;
    }

    private void SyncData()
    {
        SlimeData data = unit.Data;
        SlimeSkillData skill = data != null ? data.skill : null;

        if (data != previousData || skill != previousSkill)
            ResetState();
    }

    private bool CanUseSkill()
    {
        SlimeData data = unit.Data;

        return data != null &&
               data.skill != null &&
               (data.grade == SlimeGrade.Hero ||
                data.grade == SlimeGrade.Legendary) &&
               unit.CurrentCell != null &&
               unit.CurrentCell.Occupant == gameObject;
    }

    private void Update()
    {
        if (unit.IsDispatched)
            return;
        if (Time.timeScale == 0f)
            return;

        SyncData();

        if (!CanUseSkill())
            return;

        SlimeData data = unit.Data;
        SlimeSkillData skill = data.skill;

        float speedMultiplier = SynergyManager.Instance != null
            ? SynergyManager.Instance.GetSkillSpeedMultiplier(
                data.element
            )
            : 1f;

        cooldownRemaining = Mathf.Max(
            0f,
            cooldownRemaining - speedMultiplier * Time.deltaTime
        );

        // 횟수·확률 방식은 기본 공격 알림에서 처리합니다.
        if (skill.triggerType != SkillTriggerType.Mana)
            return;

        float cost = Mathf.Max(1f, skill.manaCost);

        mana = Mathf.Min(
            cost,
            mana + skill.manaRecovery *
                speedMultiplier * Time.deltaTime
        );

        if (mana < cost || cooldownRemaining > 0f)
            return;

        EnemyHealth target = FindNearestEnemy(data.attackRange);

        if (target == null)
            return;

        mana -= cost;
        Activate(skill, target.transform.position, target);
    }

    // SlimeAttack이 실제 기본 공격을 한 직후 호출합니다.
    public void NotifyBasicAttack(
    Vector3 targetPosition,
    EnemyHealth attackTarget = null)
    {
        if (unit.IsDispatched)
            return;
        if (!isActiveAndEnabled || Time.timeScale == 0f)
            return;

        SyncData();

        if (!CanUseSkill())
            return;

        SlimeSkillData skill = unit.Data.skill;

        if (skill.triggerType == SkillTriggerType.AttackCount)
        {
            int required = Mathf.Max(1, skill.attacksRequired);

            // 쿨타임 중에도 횟수는 최대 필요 횟수까지 쌓습니다.
            attackCount = Mathf.Min(required, attackCount + 1);

            if (attackCount >= required && cooldownRemaining <= 0f)
            {
                attackCount = 0;
                Activate(skill, targetPosition, attackTarget);
            }
        }
        else if (skill.triggerType == SkillTriggerType.Chance)
        {
            if (cooldownRemaining <= 0f &&
                Random.value < skill.triggerChance)
            {
                Activate(skill, targetPosition, attackTarget);
            }
        }
    }

    private void Activate(
    SlimeSkillData skill,
    Vector3 center,
    EnemyHealth primaryTarget = null)
    {
        cooldownRemaining = Mathf.Max(0f, skill.cooldown);

        if (skill.effectType == SkillEffectType.ChainLightning)
        {
            if (skill.chainLightningPrefab == null)
            {
                Debug.LogError("연쇄 번개 프리팹을 연결해주세요.", this);
                return;
            }

            if (primaryTarget == null || primaryTarget.IsDead)
                primaryTarget = FindNearestEnemy(unit.Data.attackRange);

            if (primaryTarget == null)
                return;

            ChainLightningSkill chain = Instantiate(
                skill.chainLightningPrefab,
                transform.position,
                Quaternion.identity
            );

            chain.Initialize(
                transform.position,
                primaryTarget,
                unit.AttackDamage * skill.damageMultiplier,
                skill.chainTargetCount,
                skill.chainJumpRange,
                skill.chainDamageRetention,
                skill.chainJumpDelay,
                skill.effectColor
            );

            return;
        }

        if (skill.effectType == SkillEffectType.GroundArea)
        {
            if (skill.groundAreaPrefab == null)
            {
                Debug.LogError("장판 프리팹을 연결해주세요.", this);
                return;
            }

            GroundSkillArea area = Instantiate(
                skill.groundAreaPrefab,
                center,
                Quaternion.identity
            );

            area.Initialize(
    center,
    unit.AttackDamage * skill.damageMultiplier,
    skill.effectRadius,
    skill.groundDuration,
    skill.groundTickInterval,
    skill.stunDuration,
    skill.effectColor,
    skill.effectPrefab
);
            if (skill.effectPrefab != null)
            {
                SlimeSkillEffect effect = Instantiate(
                    skill.effectPrefab,
                    center,
                    Quaternion.identity
                );

                effect.Initialize(
                    skill.effectRadius,
                    skill.effectColor
                );
            }


                return;
        }

        if (skill.effectType == SkillEffectType.Meteor)
        {
            if (skill.meteorPrefab == null)
            {
                Debug.LogError("메테오 프리팹을 연결해주세요.", this);
                return;
            }

            MeteorImpact meteor = Instantiate(
                skill.meteorPrefab,
                center,
                Quaternion.identity
            );

            meteor.Initialize(
                center,
                unit.AttackDamage * skill.damageMultiplier,
                skill.effectRadius,
                skill.stunDuration,
                skill.meteorFallTime,
                skill.effectPrefab,
                skill.effectColor
            );

            return;
        }

        

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

    [ContextMenu("테스트: 스킬 상태")]
    private void LogState()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"공격 횟수: {attackCount}, " +
            $"마나: {mana:0.##}, " +
            $"남은 쿨타임: {cooldownRemaining:0.##}초",
            this
        );
    }
}
