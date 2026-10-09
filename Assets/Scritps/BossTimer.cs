using UnityEngine;
using System;



[RequireComponent(typeof(EnemyHealth))]
public class BossTimer : MonoBehaviour
{
    public float RemainingTime { get; private set; }

    public bool IsAlive =>
        health != null &&
        !health.IsDead &&
        gameObject.activeInHierarchy;

    private EnemyHealth health;
    private Action onTimeout;
    private bool initialized;
    private bool expired;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    public void Initialize(float duration, Action timeoutCallback)
    {
        RemainingTime = Mathf.Max(0.1f, duration);
        onTimeout = timeoutCallback;
        initialized = true;
        expired = false;
    }

    private void Update()
    {
        if (!initialized || expired || !IsAlive)
            return;

        RemainingTime = Mathf.Max(
            0f,
            RemainingTime - Time.deltaTime
        );

        if (RemainingTime <= 0f)
        {
            expired = true;
            onTimeout?.Invoke();
        }
    }
}
