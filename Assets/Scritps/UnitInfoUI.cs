using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUI : MonoBehaviour
{
    [SerializeField] private BoardInput boardInput;
    [SerializeField] private GameObject panel;

    [SerializeField] private TMP_Text unitNameText;
    [SerializeField] private TMP_Text unitStatsText;
    [SerializeField] private TMP_Text unitSkillText;

    [SerializeField] private SlimeSummoner summoner;
    [SerializeField] private Button sellButton;
    [SerializeField] private TMP_Text sellButtonText;

    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonText;

    [SerializeField] private Button infoToggleButton;
    [SerializeField] private TMP_Text infoToggleText;

    private bool detailsOpen;
    private SlimeUnit observedUnit;

    private void Awake()
    {
        SetVisible(false);
    }

    private void LateUpdate()
    {
        SlimeUnit selected = UpdateSelection();

        if (infoToggleButton != null)
            infoToggleButton.interactable = selected != null;

        if (infoToggleText != null)
        {
            infoToggleText.text = detailsOpen
                ? "정보 닫기"
                : "정보 보기";
        }

        bool visible = selected != null && detailsOpen;
        SetVisible(visible);

        if (visible)
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
        RefreshUpgradeButton(unit);

        if (sellButton != null)
        {
            sellButton.interactable =
                summoner != null &&
                boardInput != null &&
                boardInput.isActiveAndEnabled;
        }

        if (sellButtonText != null)
        {
            sellButtonText.text = summoner != null
                ? $"판매 {summoner.GetSellPrice(unit)}G"
                : "판매";
        }

        if (unitNameText != null)
        {
            string upgradeLabel = data.grade == SlimeGrade.Legendary
                ? $" +{unit.UpgradeLevel}"
                : "";

            unitNameText.text =
                $"{data.unitName}{upgradeLabel}\n" +
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

    public void SellSelected()
    {
        if (summoner == null ||
            boardInput == null ||
            !boardInput.isActiveAndEnabled)
        {
            return;
        }

        SlimeUnit selected = boardInput.SelectedUnit;

        if (selected == null)
            return;

        if (summoner.TrySell(selected))
        {
            boardInput.ClearSelection();
            SetVisible(false);
        }
    }

    private void RefreshUpgradeButton(SlimeUnit unit)
    {
        bool legendary =
            unit.Data.grade == SlimeGrade.Legendary;

        if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(legendary);

            upgradeButton.interactable =
                legendary &&
                unit.CanUpgrade &&
                summoner != null &&
                summoner.CurrentGold >= unit.UpgradeCost &&
                boardInput != null &&
                boardInput.isActiveAndEnabled;
        }

        if (upgradeButtonText != null && legendary)
        {
            upgradeButtonText.text = unit.CanUpgrade
                ? $"강화 +{unit.UpgradeLevel + 1} · {unit.UpgradeCost}G"
                : $"최대 강화 +{unit.UpgradeLevel}";
        }
    }

    public void UpgradeSelected()
    {
        if (summoner == null ||
            boardInput == null ||
            !boardInput.isActiveAndEnabled)
        {
            return;
        }

        SlimeUnit selected = boardInput.SelectedUnit;

        if (selected != null && summoner.TryUpgrade(selected))
            Refresh(selected);
    }

    private SlimeUnit UpdateSelection()
    {
        SlimeUnit selected = null;

        if (boardInput != null && boardInput.isActiveAndEnabled)
            selected = boardInput.SelectedUnit;

        if (selected != null &&
            (!selected.isActiveAndEnabled || selected.Data == null))
        {
            selected = null;
        }

        // 선택이 바뀌거나 해제되면 정보창을 닫습니다.
        if (observedUnit != selected)
        {
            observedUnit = selected;
            detailsOpen = false;
        }

        return selected;
    }

    public void ToggleDetails()
    {
        SlimeUnit selected = UpdateSelection();

        if (selected == null)
            return;

        detailsOpen = !detailsOpen;
        SetVisible(detailsOpen);

        if (detailsOpen)
            Refresh(selected);
    }
}
