using UnityEngine;
using UnityEngine.EventSystems;



[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BoardCell : MonoBehaviour, IPointerClickHandler,
    IPointerDownHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    public GameObject Occupant { get; private set; }
    public bool IsEmpty => Occupant == null && !IsLocked;


    [SerializeField]
    private Color selectedColor =
        new Color(1f, 0.85f, 0.3f);

    private SpriteRenderer cellRenderer;
    private Color originalColor;

    [Header("스와이프 판매")]
    [SerializeField, Min(0f)]
    private float minimumSwipePixels = 60f;

    [SerializeField, Range(0f, 1f)]
    private float swipeScreenHeightRatio = 0.08f;

    private bool wasDragged;
    private SlimeUnit swipeUnit;

    private void Awake()
    {
        cellRenderer = GetComponent<SpriteRenderer>();
        originalColor = cellRenderer.color;
    }

    public bool TryPlaceUnit(GameObject unit)
    {
        if (unit == null || !IsEmpty)
            return false;

        Occupant = unit;
        unit.transform.position = transform.position;

        return true;
    }

    public void ClearUnit()
    {
        Occupant = null;
    }

    public void SetSelected(bool selected)
    {
        if (IsLocked)
        {
            cellRenderer.color = originalColor * 0.5f;
            return;
        }

        cellRenderer.color = selected
            ? selectedColor
            : originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (wasDragged ||
            eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (BoardInput.Instance != null)
            BoardInput.Instance.HandleCellClick(this);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        wasDragged = false;
        swipeUnit = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        wasDragged = true;
        eventData.eligibleForClick = false;

        BoardInput input = BoardInput.Instance;

        if (input == null || !input.isActiveAndEnabled)
            return;

        SlimeUnit selected = input.SelectedUnit;

        // 이미 선택한 유닛의 칸에서 시작한 드래그만 허용합니다.
        if (selected != null &&
            selected.CurrentCell == this &&
            Occupant == selected.gameObject)
        {
            swipeUnit = selected;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // IDragHandler가 있어야 드래그 이벤트를 받을 수 있습니다.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        eventData.eligibleForClick = false;

        SlimeUnit unit = swipeUnit;
        swipeUnit = null;

        if (unit == null)
            return;

        // 화면 좌표는 아래로 움직이면 Y가 감소합니다.
        Vector2 movement =
            eventData.position - eventData.pressPosition;

        float minimumDistance = Mathf.Max(
            minimumSwipePixels,
            Screen.height * swipeScreenHeightRatio
        );

        bool movedDownEnough = movement.y <= -minimumDistance;

        bool mostlyVertical =
            Mathf.Abs(movement.y) >= Mathf.Abs(movement.x) * 1.2f;

        if (!movedDownEnough || !mostlyVertical)
            return;

        BoardInput input = BoardInput.Instance;

        if (input != null)
            input.TrySwipeSell(unit);
    }

    public bool IsLocked { get; private set; }

    public void SetLocked(bool locked)
    {
        IsLocked = locked;
        SetSelected(false);
    }
}
