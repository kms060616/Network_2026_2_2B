using System.Collections.Generic;
using UnityEngine;

public class SlimeSummoner : MonoBehaviour
{
    [SerializeField] private Transform board;
    [SerializeField] private GameObject slimePrefab;
    [SerializeField, Min(0)] private int startingGold = 100;
    [SerializeField, Min(1)] private int summonCost = 20;

    private readonly HashSet<Transform> occupiedCells = new();
    private int gold;

    private void Awake()
    {
        gold = startingGold;
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

        List<Transform> emptyCells = new();

        foreach (Transform cell in board)
        {
            // BoardGrid가 생성한 칸만 확인합니다.
            if (cell.name.StartsWith("Cell_") &&
                !occupiedCells.Contains(cell))
            {
                emptyCells.Add(cell);
            }
        }

        if (emptyCells.Count == 0)
        {
            Debug.Log("소환할 빈칸이 없습니다.", this);
            return;
        }

        Transform selectedCell =
            emptyCells[Random.Range(0, emptyCells.Count)];

        // 칸의 크기에 영향을 받지 않도록 Board 아래에 생성합니다.
        Instantiate(
            slimePrefab,
            selectedCell.position,
            Quaternion.identity,
            board
        );

        occupiedCells.Add(selectedCell);
        gold -= summonCost;

        Debug.Log($"소환 완료! 남은 골드: {gold}", this);
    }
}
