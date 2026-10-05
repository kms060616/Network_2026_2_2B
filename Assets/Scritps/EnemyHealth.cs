using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public static event Action<int> EnemyDefeated;

    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField, Min(0)] private int goldReward = 10;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        if (CurrentHealth <= 0f)
        {
            // 중복 처치 보상을 막습니다.
            IsDead = true;

            EnemyDefeated?.Invoke(goldReward);
            Destroy(gameObject);
        }
    }

    [ContextMenu("테스트: 피해 10")]
    private void TestDamage()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Play 모드에서 테스트해주세요.", this);
            return;
        }

        TakeDamage(10f);
    }
}
