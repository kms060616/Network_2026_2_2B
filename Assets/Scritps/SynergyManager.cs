using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SynergyManager : MonoBehaviour
{
    public static SynergyManager Instance { get; private set; }

    [SerializeField] private Transform board;
    [SerializeField] private TMP_Text synergyText;

    [SerializeField, Min(0f)] private float attackBonus = 0.3f;
    [SerializeField, Range(0f, 1f)] private float attackPenalty = 0.1f;

    [Header("연계 시너지")]
    [SerializeField, Min(0f)]
    private float growthAttackSpeedBonus = 0.2f;

    [Header("증기 연계")]
    [SerializeField, Range(0f, 0.9f)]
    private float steamSlowAmount = 0.2f;

    [Header("지속 피해 연계")]
    [SerializeField, Min(0f)]
    private float electricDamagePerSecond = 5f;

    [SerializeField, Min(0f)]
    private float wildfireDamagePerSecond = 8f;

    [Header("폭발 연계")]
    [SerializeField, Range(0f, 1f)]
    private float explosionCriticalChance = 0.2f;

    [SerializeField, Min(1f)]
    private float explosionCriticalMultiplier = 2f;


    [Header("광합성 연계")]
    [SerializeField, Min(0f)]
    private float photosynthesisBonus = 0.2f;

    public float GetSkillSpeedMultiplier(SlimeElement element)
    {
        bool active =
            IsSynergyActive(SlimeElement.Grass) &&
            IsSynergyActive(SlimeElement.Electric);

        bool affectedUnit =
            element == SlimeElement.Grass ||
            element == SlimeElement.Electric;

        return active && affectedUnit
            ? 1f + photosynthesisBonus
            : 1f;
    }



    // 조건을 달성한 순서를 보관합니다.
    private readonly List<SlimeElement> qualifiedOrder = new();

    private readonly HashSet<SlimeGrade>[] ownedGrades =
    {
        new(),
        new(),
        new(),
        new()
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("SynergyManager는 하나만 두세요.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        if (board == null)
            return;

        Recalculate();
        UpdateText();
    }

    private void Recalculate()
    {
        foreach (HashSet<SlimeGrade> grades in ownedGrades)
            grades.Clear();

        SlimeUnit[] units =
            board.GetComponentsInChildren<SlimeUnit>();

        foreach (SlimeUnit unit in units)
        {
            if (!unit.isActiveAndEnabled ||
                unit.Data == null ||
                unit.CurrentCell == null ||
                unit.CurrentCell.Occupant != unit.gameObject)
            {
                continue;
            }

            int elementIndex = (int)unit.Data.element;
            ownedGrades[elementIndex].Add(unit.Data.grade);
        }

        // 조건을 잃은 속성을 대기 순서에서도 제거합니다.
        for (int i = qualifiedOrder.Count - 1; i >= 0; i--)
        {
            int index = (int)qualifiedOrder[i];

            if (ownedGrades[index].Count < 3)
                qualifiedOrder.RemoveAt(i);
        }

        // 새로 조건을 달성한 속성을 순서 끝에 추가합니다.
        for (int i = 0; i < ownedGrades.Length; i++)
        {
            SlimeElement element = (SlimeElement)i;

            if (ownedGrades[i].Count >= 3 &&
                !qualifiedOrder.Contains(element))
            {
                qualifiedOrder.Add(element);
            }
        }
    }

    public float GetAttackMultiplier(SlimeElement element)
    {
        if (!isActiveAndEnabled || qualifiedOrder.Count == 0)
            return 1f;

        int activeCount = Mathf.Min(2, qualifiedOrder.Count);

        for (int i = 0; i < activeCount; i++)
        {
            if (qualifiedOrder[i] == element)
                return 1f + attackBonus;
        }

        // 활성화된 두 속성 외에는 한 번만 감소를 적용합니다.
        return 1f - attackPenalty;
    }

    private void UpdateText()
    {
        if (synergyText == null)
            return;

        string text = "속성 시너지";

        // 속성별 보유 등급 수와 상태
        for (int i = 0; i < ownedGrades.Length; i++)
        {
            SlimeElement element = (SlimeElement)i;
            int position = qualifiedOrder.IndexOf(element);

            string state = position >= 0
                ? (position < 2 ? "발동" : "대기")
                : "미발동";

            text += $"\n{ElementName(element)}: " +
                $"{ownedGrades[i].Count}/3종 · {state}";
        }

        // 활성화된 연계만 추가
        string comboText = "";

        if (IsSynergyActive(SlimeElement.Water) &&
            IsSynergyActive(SlimeElement.Fire))
        {
            comboText =
                $"증기 · 적 이동속도 -{steamSlowAmount * 100f:0}%";
        }
        else if (IsSynergyActive(SlimeElement.Water) &&
                 IsSynergyActive(SlimeElement.Grass))
        {
            comboText =
                $"성장 · 물/풀 공격속도 +{growthAttackSpeedBonus * 100f:0}%";
        }
        else if (IsSynergyActive(SlimeElement.Water) &&
                 IsSynergyActive(SlimeElement.Electric))
        {
            comboText =
                $"감전 · 초당 피해 {electricDamagePerSecond:0.##}";
        }
        else if (IsSynergyActive(SlimeElement.Fire) &&
                 IsSynergyActive(SlimeElement.Grass))
        {
            comboText =
                $"들불 · 초당 피해 {wildfireDamagePerSecond:0.##}";
        }
        else if (IsSynergyActive(SlimeElement.Fire) &&
                 IsSynergyActive(SlimeElement.Electric))
        {
            comboText =
                $"폭발 · 불/전기 치명타 " +
                $"{explosionCriticalChance * 100f:0}% · " +
                $"피해 {explosionCriticalMultiplier:0.##}배";
        }
        else if (IsSynergyActive(SlimeElement.Grass) &&
                 IsSynergyActive(SlimeElement.Electric))
        {
            comboText =
                $"광합성 · 풀/전기 마나 회복·쿨타임 진행속도 " +
                $"+{photosynthesisBonus * 100f:0}%";
        }

        if (!string.IsNullOrEmpty(comboText))
            text += $"\n\n연계: {comboText}";

        synergyText.text = text;
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool IsSynergyActive(SlimeElement element)
    {
        if (!isActiveAndEnabled)
            return false;

        int position = qualifiedOrder.IndexOf(element);

        // 대기 중인 시너지는 연계에 포함하지 않습니다.
        return position >= 0 && position < 2;
    }

    public float GetAttackSpeedMultiplier(SlimeElement element)
    {
        bool growthActive =
            IsSynergyActive(SlimeElement.Water) &&
            IsSynergyActive(SlimeElement.Grass);

        bool affectedUnit =
            element == SlimeElement.Water ||
            element == SlimeElement.Grass;

        if (growthActive && affectedUnit)
            return 1f + growthAttackSpeedBonus;

        return 1f;
    }

    public float GetEnemyMoveSpeedMultiplier()
    {
        bool steamActive =
            IsSynergyActive(SlimeElement.Water) &&
            IsSynergyActive(SlimeElement.Fire);

        return steamActive ? 1f - steamSlowAmount : 1f;
    }

    public float GetEnemyDamagePerSecond()
    {
        if (IsSynergyActive(SlimeElement.Water) &&
            IsSynergyActive(SlimeElement.Electric))
        {
            return electricDamagePerSecond;
        }

        if (IsSynergyActive(SlimeElement.Fire) &&
            IsSynergyActive(SlimeElement.Grass))
        {
            return wildfireDamagePerSecond;
        }

        return 0f;
    }

    public float GetCriticalChance(SlimeElement element)
    {
        bool explosionActive =
            IsSynergyActive(SlimeElement.Fire) &&
            IsSynergyActive(SlimeElement.Electric);

        bool affectedUnit =
            element == SlimeElement.Fire ||
            element == SlimeElement.Electric;

        return explosionActive && affectedUnit
            ? explosionCriticalChance
            : 0f;
    }

    public float GetCriticalMultiplier()
    {
        return explosionCriticalMultiplier;
    }

}
