using UnityEngine;


[RequireComponent(typeof(EnemyHealth))]
public class EnemySynergyEffect : MonoBehaviour
{
    private EnemyHealth health;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void Update()
    {
        if (health.IsDead || Time.timeScale == 0f)
            return;

        SynergyManager manager = SynergyManager.Instance;

        if (manager == null)
            return;

        float damagePerSecond =
            manager.GetEnemyDamagePerSecond();

        if (damagePerSecond <= 0f)
            return;

        health.TakeDamage(damagePerSecond * Time.deltaTime);
    }

    [ContextMenu("테스트: 지속 피해 상태")]
    private void LogEffect()
    {
        if (!Application.isPlaying)
            return;

        float damagePerSecond = SynergyManager.Instance != null
            ? SynergyManager.Instance.GetEnemyDamagePerSecond()
            : 0f;

        Debug.Log(
            $"지속 피해: 초당 {damagePerSecond}, " +
            $"현재 체력: {health.CurrentHealth:0.##}",
            this
        );
    }
}
