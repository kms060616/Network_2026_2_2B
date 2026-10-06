using TMPro;
using UnityEngine;

public class BoardInput : MonoBehaviour
{
    public static BoardInput Instance { get; private set; }

    [SerializeField] private bool mergeMode = true;
    [SerializeField] private TMP_Text modeText;

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

    private void Start()
    {
        UpdateModeText();
    }

    public void ToggleMode()
    {
        ClearSelection();
        mergeMode = !mergeMode;
        UpdateModeText();
    }

    private void UpdateModeText()
    {
        if (modeText != null)
        {
            modeText.text = mergeMode
                ? "현재: 합성 모드"
                : "현재: 교체 모드";
        }
    }

    public void HandleCellClick(BoardCell clickedCell)
    {
        if (clickedCell == null)
            return;

        if (selectedUnit == null)
            ClearSelection();

        SlimeUnit clickedUnit = null;

        if (!clickedCell.IsEmpty)
        {
            clickedCell.Occupant.TryGetComponent(
                out clickedUnit
            );
        }

        if (selectedUnit == null)
        {
            if (clickedUnit != null)
                SelectUnit(clickedUnit);

            return;
        }

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
            if (mergeMode && selectedUnit.CanMergeWith(clickedUnit))
            {
                completed = selectedUnit.TryMergeInto(clickedUnit);
            }
            else
            {
                completed = selectedUnit.TrySwapWith(clickedUnit);
            }
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
