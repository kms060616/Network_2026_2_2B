using UnityEngine;

public class SlimeUnit : MonoBehaviour
{
    public BoardCell CurrentCell { get; private set; }

    public bool TryMoveTo(BoardCell destination)
    {
        if (destination == null)
            return false;

        if (destination == CurrentCell)
            return true;

        if (!destination.TryPlaceUnit(gameObject))
            return false;

        if (CurrentCell != null &&
            CurrentCell.Occupant == gameObject)
        {
            CurrentCell.ClearUnit();
            CurrentCell.SetSelected(false);
        }

        CurrentCell = destination;
        return true;
    }

    public bool TrySwapWith(SlimeUnit other)
    {
        if (other == null || other == this)
            return false;

        BoardCell myCell = CurrentCell;
        BoardCell otherCell = other.CurrentCell;

        // 두 유닛이 정상적으로 서로 다른 칸에 있는지 확인합니다.
        if (myCell == null || otherCell == null ||
            myCell == otherCell ||
            myCell.Occupant != gameObject ||
            otherCell.Occupant != other.gameObject)
        {
            return false;
        }

        myCell.ClearUnit();
        otherCell.ClearUnit();

        // 방금 비운 두 칸에 서로 바꿔 배치합니다.
        otherCell.TryPlaceUnit(gameObject);
        myCell.TryPlaceUnit(other.gameObject);

        CurrentCell = otherCell;
        other.CurrentCell = myCell;

        myCell.SetSelected(false);
        otherCell.SetSelected(false);

        return true;
    }

    private void OnDestroy()
    {
        if (CurrentCell != null &&
            CurrentCell.Occupant == gameObject)
        {
            CurrentCell.ClearUnit();
            CurrentCell.SetSelected(false);
        }
    }
}
