using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public static event Action<int> EnemyDefeated;

    private static readonly HashSet<EnemyHealth> aliveEnemies = new();

    public static int AliveCount => aliveEnemies.Count;

    [SerializeField, Min(1f)] private float maxHealth = 30f;
    [SerializeField, Min(0)] private int goldReward = 10;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    // Play를 새로 시작할 때 이전 실행의 정보를 초기화합니다.
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        aliveEnemies.Clear();
        EnemyDefeated = null;
    }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void OnEnable()
    {
        if (!IsDead)
            aliveEnemies.Add(this);
    }

    private void OnDisable()
    {
        aliveEnemies.Remove(this);
    }

    public void Configure(float health, int reward)
    {
        maxHealth = Mathf.Max(1f, health);
        goldReward = Mathf.Max(0, reward);

        CurrentHealth = maxHealth;
        IsDead = false;

        if (isActiveAndEnabled)
            aliveEnemies.Add(this);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;

            // Destroy 완료를 기다리지 않고 적 수에서 제외합니다.
            aliveEnemies.Remove(this);

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
