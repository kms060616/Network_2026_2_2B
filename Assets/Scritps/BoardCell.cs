using UnityEngine;
using UnityEngine.EventSystems;



[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BoardCell : MonoBehaviour, IPointerClickHandler
{
    public GameObject Occupant { get; private set; }
    public bool IsEmpty => Occupant == null;

    [SerializeField]
    private Color selectedColor =
        new Color(1f, 0.85f, 0.3f);

    private SpriteRenderer cellRenderer;
    private Color originalColor;

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
        cellRenderer.color = selected
            ? selectedColor
            : originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Debug.Log(
            $"칸 클릭: {name}, 유닛: {Occupant}",
            this
        );

        if (BoardInput.Instance == null)
        {
            Debug.LogWarning("씬에 BoardInput이 없습니다.", this);
            return;
        }

        BoardInput.Instance.HandleCellClick(this);
    }
}
