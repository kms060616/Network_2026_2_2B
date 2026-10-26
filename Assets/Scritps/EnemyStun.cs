using UnityEngine;

public class EnemyStun : MonoBehaviour
{
    public bool IsStunned => remainingTime > 0f;

    private float remainingTime;

    public void ApplyStun(float duration)
    {
        // 중첩해서 시간을 더하지 않고, 더 긴 남은 시간을 사용합니다.
        remainingTime = Mathf.Max(
            remainingTime,
            Mathf.Max(0f, duration)
        );
    }

    private void Update()
    {
        remainingTime = Mathf.Max(
            0f,
            remainingTime - Time.deltaTime
        );
    }
}
