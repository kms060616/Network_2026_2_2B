using UnityEngine;

public class SlimeUnit : MonoBehaviour
{
    [SerializeField] private SlimeData data;

    public SlimeData Data => data;
    public BoardCell CurrentCell { get; private set; }

    private void Awake()
    {
        if (data != null)
            ApplyAppearance();
    }

    public void Initialize(SlimeData newData)
    {
        data = newData;

        if (data == null)
        {
            Debug.LogError("슬라임 데이터가 없습니다.", this);
            return;
        }

        ApplyAppearance();
    }

    private void ApplyAppearance()
    {
        gameObject.name = $"Slime_{data.element}_{data.grade}";

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            if (data.sprite != null)
                renderer.sprite = data.sprite;

            renderer.color = data.color;
        }

        float size = data.grade switch
        {
            SlimeGrade.Common => 0.4f,
            SlimeGrade.Rare => 0.5f,
            SlimeGrade.Hero => 0.6f,
            SlimeGrade.Legendary => 0.7f,
            _ => 0.4f
        };

        transform.localScale = new Vector3(size, size, 1f);
    }

    public bool CanMergeWith(SlimeUnit other)
    {
        return other != null &&
               other != this &&
               data != null &&
               other.data != null &&
               data.element == other.data.element &&
               data.grade == other.data.grade &&
               data.grade != SlimeGrade.Legendary &&
               data.mergeResult != null;
    }

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

        if (myCell == null || otherCell == null ||
            myCell == otherCell ||
            myCell.Occupant != gameObject ||
            otherCell.Occupant != other.gameObject)
        {
            return false;
        }

        myCell.ClearUnit();
        otherCell.ClearUnit();

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

    public bool TryMergeInto(SlimeUnit other)
    {
        if (!CanMergeWith(other))
            return false;

        BoardCell sourceCell = CurrentCell;
        BoardCell destinationCell = other.CurrentCell;

        if (sourceCell == null || destinationCell == null ||
            sourceCell == destinationCell ||
            sourceCell.Occupant != gameObject ||
            destinationCell.Occupant != other.gameObject)
        {
            return false;
        }

        SlimeData result = data.mergeResult;
        SlimeGrade nextGrade = (SlimeGrade)((int)data.grade + 1);

        // 합성 결과 설정이 잘못되면 합성을 취소합니다.
        if (result.element != data.element ||
            result.grade != nextGrade ||
            other.data.mergeResult != result)
        {
            Debug.LogWarning(
                "두 유닛의 Merge Result를 같은 속성의 다음 등급으로 연결해주세요.",
                this
            );
            return false;
        }

        // 두 번째로 누른 유닛을 상위 유닛으로 변경합니다.
        other.Initialize(result);

        // 처음 선택한 유닛의 칸을 비우고 유닛을 제거합니다.
        sourceCell.ClearUnit();
        sourceCell.SetSelected(false);
        CurrentCell = null;

        // Destroy가 완료되기 전 추가 공격과 입력을 막습니다.
        gameObject.SetActive(false);
        Destroy(gameObject);

        return true;
    }
}
