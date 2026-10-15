using UnityEngine;


[CreateAssetMenu(
    fileName = "SlimeSkill",
    menuName = "Slime Defense/Slime Skill"
)]
public class SlimeSkillData : ScriptableObject
{
    public string skillName;

    [Min(1f)] public float manaCost = 100f;
    [Min(0f)] public float manaRecovery = 10f;
    [Min(0f)] public float cooldown = 8f;

    [Min(0f)] public float damageMultiplier = 3f;
    [Min(0f)] public float effectRadius = 1f;
}
