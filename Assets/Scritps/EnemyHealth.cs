using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxHealth = 30f;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // 죽은 적이나 0 이하의 피해는 처리하지 않습니다.
        if (IsDead || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        Debug.Log($"적 체력: {CurrentHealth}/{maxHealth}", this);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            Destroy(gameObject);
        }
    }

    // 자동 공격을 만들기 전에 체력 감소를 테스트합니다.
    [ContextMenu("테스트: 피해 10")]
    private void TestDamage()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 버튼을 누른 뒤 테스트해주세요.", this);
            return;
        }

        Debug.Log("피해 테스트 실행", this);
        TakeDamage(10f);
    }
}
