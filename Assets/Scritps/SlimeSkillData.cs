using UnityEngine;


public enum SkillTriggerType
{
    Mana = 0,
    AttackCount = 1,
    Chance = 2
}

[CreateAssetMenu(
    fileName = "SlimeSkill",
    menuName = "Slime Defense/Slime Skill"
)]
public class SlimeSkillData : ScriptableObject
{
    public string skillName;
    public SkillTriggerType triggerType = SkillTriggerType.Mana;

    [Header("공격 횟수 방식")]
    [Min(1)] public int attacksRequired = 5;

    [Header("확률 방식")]
    [Range(0f, 1f)] public float triggerChance = 0.2f;

    [Header("마나 방식")]
    [Min(1f)] public float manaCost = 100f;
    [Min(0f)] public float manaRecovery = 10f;

    [Header("공통 설정")]
    [Min(0f)] public float cooldown = 8f;
    [Min(0f)] public float damageMultiplier = 3f;
    [Min(0f)] public float effectRadius = 1f;

    [Header("발동 이펙트")]
    public SlimeSkillEffect effectPrefab;
    public Color effectColor = new Color(0.3f, 0.8f, 1f, 1f);

    [Header("효과 종류")]
    public SkillEffectType effectType = SkillEffectType.AreaDamage;

    [Header("메테오")]
    public MeteorImpact meteorPrefab;

    [Min(0.01f)]
    public float meteorFallTime = 0.6f;

    [Min(0f)]
    public float stunDuration = 1.5f;

    [Header("장판")]
    public GroundSkillArea groundAreaPrefab;

    [Min(0.1f)]
    public float groundDuration = 4f;

    [Min(0.1f)]
    public float groundTickInterval = 0.5f;

    [Header("연쇄 번개")]
    public ChainLightningSkill chainLightningPrefab;

    [Min(1)]
    public int chainTargetCount = 5;

    [Min(0.1f)]
    public float chainJumpRange = 1.5f;

    [Range(0f, 1f)]
    public float chainDamageRetention = 0.8f;

    [Min(0.01f)]
    public float chainJumpDelay = 0.07f;
}

public enum SkillEffectType
{
    AreaDamage = 0,
    Meteor = 1,
    GroundArea = 2,
    ChainLightning = 3
}
