using UnityEngine;

[CreateAssetMenu(
    fileName = "SlimeData",
    menuName = "Slime Defense/Slime Data"
)]
public class SlimeData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitName;
    public SlimeElement element;
    public SlimeGrade grade;

    [Header("외형")]
    public Sprite sprite;
    public Color color = Color.white;

    [Header("기본 공격")]
    [Min(0f)] public float attackDamage = 10f;
    [Min(0.01f)] public float attackInterval = 1f;
    [Min(0f)] public float attackRange = 3f;

    [Header("합성 결과")]
    public SlimeData mergeResult;
}
