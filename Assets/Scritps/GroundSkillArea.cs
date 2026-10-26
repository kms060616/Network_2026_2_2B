using System.Collections.Generic;
using UnityEngine;

public class GroundSkillArea : MonoBehaviour
{
    [SerializeField] private SpriteRenderer areaRenderer;

    private readonly HashSet<EnemyHealth> stunnedEnemies = new();

    private float damagePerTick;
    private float radiusSquared;
    private float duration;
    private float tickInterval;
    private float stunDuration;

    private float elapsed;
    private float nextTickTime;
    private bool initialized;

    private SlimeSkillEffect tickEffectPrefab;
    private Color tickEffectColor;
    private float effectRadius;

    [SerializeField, Range(0f, 1f)]
    private float areaOpacity = 0.65f;

    public void Initialize(
        Vector3 center,
        float damage,
        float radius,
        float areaDuration,
        float interval,
        float stunTime,
        Color color,
        SlimeSkillEffect effectPrefab)
    {
        transform.position = center;

        damagePerTick = Mathf.Max(0f, damage);
        radius = Mathf.Max(0f, radius);
        radiusSquared = radius * radius;

        duration = Mathf.Max(0.01f, areaDuration);
        tickInterval = Mathf.Max(0.1f, interval);
        stunDuration = Mathf.Max(0f, stunTime);

        elapsed = 0f;
        nextTickTime = tickInterval;
        stunnedEnemies.Clear();

        SetupVisual(radius, color);
        
        initialized = true;

        // 생성 시 범위 안의 적에게 첫 피해와 스턴을 적용합니다.
        EnemyHealth[] enemies = FindEnemies();

        ApplyEntryStuns(enemies);

        tickEffectPrefab = effectPrefab;
        tickEffectColor = color;
        effectRadius = radius;
        ApplyDamage(enemies);

    }

    private void Update()
    {
        if (!initialized || Time.timeScale == 0f)
            return;

        elapsed += Time.deltaTime;

        EnemyHealth[] enemies = FindEnemies();

        if (elapsed < duration)
            ApplyEntryStuns(enemies);

        // 생성 직후 피해와 중복되지 않도록 다음 주기부터 처리합니다.
        while (nextTickTime <= elapsed &&
               nextTickTime < duration)
        {
            ApplyDamage(enemies);
            nextTickTime += tickInterval;
        }

        if (elapsed >= duration)
        {
            initialized = false;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private EnemyHealth[] FindEnemies()
    {
        return FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );
    }

    private bool IsInside(EnemyHealth enemy)
    {
        if (enemy == null ||
            enemy.IsDead ||
            !enemy.isActiveAndEnabled)
        {
            return false;
        }

        Vector2 offset =
            enemy.transform.position - transform.position;

        return offset.sqrMagnitude <= radiusSquared;
    }

    private void ApplyEntryStuns(EnemyHealth[] enemies)
    {
        if (stunDuration <= 0f)
            return;

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsInside(enemy) || stunnedEnemies.Contains(enemy))
                continue;

            if (enemy.TryGetComponent(out EnemyStun stun))
            {
                stun.ApplyStun(stunDuration);
                stunnedEnemies.Add(enemy);
            }
        }
    }

    private void ApplyDamage(EnemyHealth[] enemies)
    {
        if (tickEffectPrefab != null)
        {
            SlimeSkillEffect effect = Instantiate(
                tickEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            effect.Initialize(effectRadius, tickEffectColor);
        }

        foreach (EnemyHealth enemy in enemies)
        {
            if (IsInside(enemy))
                enemy.TakeDamage(damagePerTick);
        }
    }

    private void SetupVisual(float radius, Color color)
    {
        if (areaRenderer == null || areaRenderer.sprite == null)
            return;

        Vector3 size = areaRenderer.sprite.bounds.size;

        if (size.x <= 0f || size.y <= 0f)
            return;

        // 원형 이미지의 지름을 실제 피해 범위에 맞춥니다.
        areaRenderer.transform.localScale = new Vector3(
            radius * 2f / size.x,
            radius * 2f / size.y,
            1f
        );

        color.a = areaOpacity;
        areaRenderer.color = color;
    }
}
