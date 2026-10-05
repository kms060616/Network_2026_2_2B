using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SlimeSummoner : MonoBehaviour
{
    [SerializeField] private Transform board;
    [SerializeField] private SlimeUnit slimePrefab;
    [SerializeField] private TMP_Text goldText;
    [SerializeField, Min(0)] private int startingGold = 100;
    [SerializeField, Min(1)] private int summonCost = 20;

    private int gold;

    private void Awake()
    {
        gold = startingGold;
        UpdateGoldText();
    }

    private void OnEnable()
    {
        EnemyHealth.EnemyDefeated += AddGold;
    }

    private void OnDisable()
    {
        EnemyHealth.EnemyDefeated -= AddGold;
    }

    private void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        if (goldText != null)
            goldText.text = $"골드: {gold}";
    }

    public void Summon()
    {
        if (board == null || slimePrefab == null)
        {
            Debug.LogError("Board와 Slime 프리팹을 연결해주세요.", this);
            return;
        }

        if (gold < summonCost)
        {
            Debug.Log("골드가 부족합니다.", this);
            return;
        }

        List<BoardCell> emptyCells = new();

        foreach (Transform child in board)
        {
            if (child.TryGetComponent(out BoardCell cell) &&
                cell.IsEmpty)
            {
                emptyCells.Add(cell);
            }
        }

        if (emptyCells.Count == 0)
        {
            Debug.Log("소환할 빈칸이 없습니다.", this);
            return;
        }

        BoardCell selectedCell =
            emptyCells[Random.Range(0, emptyCells.Count)];

        SlimeUnit unit = Instantiate(
            slimePrefab,
            selectedCell.transform.position,
            Quaternion.identity,
            board
        );

        if (!unit.TryMoveTo(selectedCell))
        {
            Destroy(unit.gameObject);
            return;
        }

        gold -= summonCost;
        UpdateGoldText();
    }
}
