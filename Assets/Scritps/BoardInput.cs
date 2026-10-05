using UnityEngine;

public class BoardInput : MonoBehaviour
{
    public static BoardInput Instance { get; private set; }

    private SlimeUnit selectedUnit;
    private BoardCell selectedCell;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("BoardInput은 씬에 하나만 두세요.", this);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void HandleCellClick(BoardCell clickedCell)
    {
        if (clickedCell == null)
            return;

        // 선택 중인 유닛이 삭제됐다면 선택 표시도 정리합니다.
        if (selectedUnit == null)
            ClearSelection();

        SlimeUnit clickedUnit = null;

        if (!clickedCell.IsEmpty)
        {
            clickedCell.Occupant.TryGetComponent(
                out clickedUnit
            );
        }

        // 선택된 유닛이 없으면 클릭한 유닛을 선택합니다.
        if (selectedUnit == null)
        {
            if (clickedUnit != null)
                SelectUnit(clickedUnit);

            return;
        }

        // 같은 칸을 다시 누르면 선택을 해제합니다.
        if (clickedCell == selectedCell)
        {
            ClearSelection();
            return;
        }

        bool completed = false;

        if (clickedCell.IsEmpty)
        {
            completed = selectedUnit.TryMoveTo(clickedCell);
        }
        else if (clickedUnit != null)
        {
            completed = selectedUnit.TrySwapWith(clickedUnit);
        }

        if (completed)
            ClearSelection();
    }

    private void SelectUnit(SlimeUnit unit)
    {
        ClearSelection();

        if (unit.CurrentCell == null)
            return;

        selectedUnit = unit;
        selectedCell = unit.CurrentCell;
        selectedCell.SetSelected(true);
    }

    private void ClearSelection()
    {
        if (selectedCell != null)
            selectedCell.SetSelected(false);

        selectedUnit = null;
        selectedCell = null;
    }

    private void OnDisable()
    {
        ClearSelection();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
