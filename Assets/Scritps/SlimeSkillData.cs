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
}
