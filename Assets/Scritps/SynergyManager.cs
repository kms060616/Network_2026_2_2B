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
}
