using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform[] points;
    [SerializeField, Min(0f)] private float moveSpeed = 1.5f;

    private int targetIndex;

    private EnemyStun stun;

    public float CurrentMoveSpeed
    {
        get
        {
            float multiplier = SynergyManager.Instance != null
                ? SynergyManager.Instance
                    .GetEnemyMoveSpeedMultiplier()
                : 1f;

            return moveSpeed * multiplier;
        }
    }

    public void SetPath(Transform[] pathPoints)
    {
        points = pathPoints;
    }

    private void Start()
    {
        if (points == null || points.Length < 2)
        {
            Debug.LogError(
                "경로 지점을 2개 이상 연결해주세요.",
                this
            );

            enabled = false;
            return;
        }

        foreach (Transform point in points)
        {
            if (point == null)
            {
                Debug.LogError(
                    "비어 있는 경로 지점이 있습니다.",
                    this
                );

                enabled = false;
                return;
            }
        }

        transform.position = points[0].position;
        targetIndex = 1;
    }

    private void Update()
    {
        if (stun != null && stun.IsStunned)
            return;

        Vector3 destination = points[targetIndex].position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            CurrentMoveSpeed * Time.deltaTime
        );

        if ((transform.position - destination).sqrMagnitude < 0.0001f)
        {
            transform.position = destination;
            targetIndex = (targetIndex + 1) % points.Length;
        }
    }

    [ContextMenu("테스트: 현재 이동속도")]
    private void LogCurrentMoveSpeed()
    {
        if (!Application.isPlaying)
            return;

        Debug.Log(
            $"기본 속도: {moveSpeed}, 현재 속도: {CurrentMoveSpeed}",
            this
        );
    }

    private void Awake()
    {
        stun = GetComponent<EnemyStun>();
    }
}
