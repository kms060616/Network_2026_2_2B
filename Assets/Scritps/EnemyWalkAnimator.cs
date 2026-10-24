using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]

public class EnemyWalkAnimator : MonoBehaviour
{
    [SerializeField] private EnemyWalkData data;

    private SpriteRenderer spriteRenderer;
    private Vector3 previousPosition;

    private int direction;
    private float animationTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        previousPosition = transform.position;

        if (data != null)
            Configure(data);
    }

    public void Configure(EnemyWalkData newData)
    {
        data = newData;
        animationTime = 0f;
        direction = 3; // 오른쪽에서 시작
        previousPosition = transform.position;

        if (data == null)
            return;

        if (!ValidFrames(data.down) ||
            !ValidFrames(data.up) ||
            !ValidFrames(data.left) ||
            !ValidFrames(data.right))
        {
            Debug.LogError(
                "걷기 데이터의 네 방향 이미지를 모두 연결해주세요.",
                this
            );

            data = null;
            return;
        }

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        spriteRenderer.sprite = data.right[0];
    }

    private static bool ValidFrames(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0)
            return false;

        foreach (Sprite frame in frames)
        {
            if (frame == null)
                return false;
        }

        return true;
    }

    private void LateUpdate()
    {
        Vector3 movement = transform.position - previousPosition;
        previousPosition = transform.position;

        if (data == null || Time.timeScale == 0f)
            return;

        Vector2 movement2D = movement;

        // 멈춰 있으면 현재 자세를 유지합니다.
        if (movement2D.sqrMagnitude < 0.0000001f)
            return;

        int nextDirection;

        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            nextDirection = movement.x > 0f ? 3 : 2;
        }
        else
        {
            nextDirection = movement.y > 0f ? 1 : 0;
        }

        if (direction != nextDirection)
        {
            direction = nextDirection;
            animationTime = 0f;
        }

        float speedMultiplier = SynergyManager.Instance != null
            ? SynergyManager.Instance.GetEnemyMoveSpeedMultiplier()
            : 1f;

        animationTime += Time.deltaTime *
            Mathf.Max(1f, data.framesPerSecond) *
            speedMultiplier;

        Sprite[] frames = GetFrames();
        animationTime = Mathf.Repeat(animationTime, frames.Length);

        int frameIndex = Mathf.FloorToInt(animationTime);
        spriteRenderer.sprite = frames[frameIndex];
    }

    private Sprite[] GetFrames()
    {
        return direction switch
        {
            0 => data.down,
            1 => data.up,
            2 => data.left,
            _ => data.right
        };
    }
}
