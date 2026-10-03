using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;

    private int targetIndex;

    private void Start()
    {
        if (points == null || points.Length < 2)
        {
            Debug.LogError("경로 지점을 2개 이상 연결해주세요.", this);
            enabled = false;
            return;
        }

        foreach (Transform point in points)
        {
            if (point == null)
            {
                Debug.LogError("비어 있는 경로 지점이 있습니다.", this);
                enabled = false;
                return;
            }
        }

        // 첫 지점에서 출발해 두 번째 지점으로 이동합니다.
        transform.position = points[0].position;
        targetIndex = 1;
    }

    private void Update()
    {
        Vector3 destination = points[targetIndex].position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            moveSpeed * Time.deltaTime
        );

        // 도착하면 다음 지점으로 이동합니다.
        if ((transform.position - destination).sqrMagnitude < 0.0001f)
        {
            transform.position = destination;
            targetIndex = (targetIndex + 1) % points.Length;
        }
    }
}
