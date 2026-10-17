using TMPro;
using UnityEngine;

public class UnitInfoUI : MonoBehaviour
{
    [SerializeField] private BoardInput boardInput;
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text unitStatsText;
    [SerializeField] private TMP_Text unitSkillText;

    private void Awake()
    {
        SetVisible(false);
    }

    private void LateUpdate()
    {
        SlimeUnit selected = null;

        if (boardInput != null && boardInput.isActiveAndEnabled)
            selected = boardInput.SelectedUnit;

        if (selected == null ||
            !selected.isActiveAndEnabled ||
            selected.Data == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        Refresh(selected);
    }

    private void SetVisible(bool visible)
    {
        if (panel != null && panel.activeSelf != visible)
            panel.SetActive(visible);
    }

    private void Refresh(SlimeUnit unit)
    {
        SlimeData data = unit.Data;

        if (unitNameText != null)
        {
            unitNameText.text =
                $"{data.unitName}\n" +
                $"{ElementName(data.element)} / {GradeName(data.grade)}";
        }

        if (unitStatsText != null)
        {
            unitStatsText.text =
                $"공격력: {unit.AttackDamage:0.##}\n" +
                $"공격 간격: {unit.AttackInterval:0.##}초\n" +
                $"사거리: {data.attackRange:0.##}";
        }

        if (unitSkillText == null)
            return;

        bool skillGrade =
            data.grade == SlimeGrade.Hero ||
            data.grade == SlimeGrade.Legendary;

        if (!skillGrade || data.skill == null)
        {
            unitSkillText.text = "고유 스킬: 없음";
            return;
        }

        if (!unit.TryGetComponent(out SlimeSkillRunner runner) ||
            !runner.isActiveAndEnabled)
        {
            unitSkillText.text = "스킬 실행 컴포넌트를 확인해주세요.";
            return;
        }

        SlimeSkillData skill = data.skill;

        string state = skill.triggerType switch
        {
            SkillTriggerType.AttackCount =>
                $"공격 횟수: {runner.AttackCount}/" +
                $"{Mathf.Max(1, skill.attacksRequired)}",

            SkillTriggerType.Chance =>
                $"공격 시 발동 확률: {skill.triggerChance * 100f:0.#}%",

            SkillTriggerType.Mana =>
                $"마나: {runner.CurrentMana:0.#}/" +
                $"{Mathf.Max(1f, skill.manaCost):0.#}",

            _ => ""
        };

        string cooldownState = runner.RemainingCooldown > 0f
            ? $"남은 쿨타임: {runner.RemainingCooldown:0.0}초"
            : "쿨타임: 준비 완료";

        unitSkillText.text =
            $"스킬: {skill.skillName}\n" +
            $"{state}\n" +
            $"{cooldownState}";
    }

    private static string ElementName(SlimeElement element)
    {
        return element switch
        {
            SlimeElement.Water => "물",
            SlimeElement.Fire => "불",
            SlimeElement.Grass => "풀",
            SlimeElement.Electric => "전기",
            _ => element.ToString()
        };
    }

    private static string GradeName(SlimeGrade grade)
    {
        return grade switch
        {
            SlimeGrade.Common => "일반",
            SlimeGrade.Rare => "희귀",
            SlimeGrade.Hero => "영웅",
            SlimeGrade.Legendary => "전설",
            _ => grade.ToString()
        };
    }
}
