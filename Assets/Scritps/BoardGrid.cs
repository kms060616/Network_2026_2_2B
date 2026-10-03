using UnityEngine;

public class BoardGrid : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private int columns = 3;
    [SerializeField] private int rows = 6;
    [SerializeField] private float spacing = 1f;

    private void Start()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("BoardGrid에 Cell 프리팹을 연결해주세요.", this);
            return;
        }

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.name = $"Cell_{row}_{column}";

                // Board 위치를 중심으로 정렬합니다.
                float x = (column - (columns - 1) * 0.5f) * spacing;
                float y = ((rows - 1) * 0.5f - row) * spacing;

                cell.transform.localPosition = new Vector3(x, y, 0f);
            }
        }
    }
}
