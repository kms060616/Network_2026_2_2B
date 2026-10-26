using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterBombSkill : MonoBehaviour
{
    [SerializeField] private SlimeProjectile dropletPrefab;

    public void Initialize(
        Vector3 center,
        float attackDamage,
        float explosionMultiplier,
        float explosionRadius,
        int dropletCount,
        float dropletMultiplier,
        float searchRadius,
        float followUpDelay,
        SlimeSkillEffect explosionEffect,
        Color color)
    {
        transform.position = center;

        StartCoroutine(Execute(
            center,
            attackDamage,
            explosionMultiplier,
            explosionRadius,
            dropletCount,
            dropletMultiplier,
            searchRadius,
            followUpDelay,
            explosionEffect,
            color
        ));
    }

    private IEnumerator Execute(
        Vector3 center,
        float attackDamage,
        float explosionMultiplier,
        float explosionRadius,
        int dropletCount,
        float dropletMultiplier,
        float searchRadius,
        float followUpDelay,
        SlimeSkillEffect explosionEffect,
        Color color)
    {
        // 첫 번째 물폭탄 연출
        if (explosionEffect != null)
        {
            SlimeSkillEffect effect = Instantiate(
                explosionEffect,
                center,
                Quaternion.identity
            );

            effect.Initialize(explosionRadius, color);
        }

        // 첫 번째 범위 피해
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        float radiusSquared = explosionRadius * explosionRadius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsAlive(enemy))
                continue;

            Vector2 offset = enemy.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
            {
                enemy.TakeDamage(
                    attackDamage * explosionMultiplier
                );
            }
        }

        if (dropletCount <= 0)
        {
            Destroy(gameObject);
            yield break;
        }

        if (dropletPrefab == null)
        {
            Debug.LogError(
                "WaterBombSkill에 물방울 프리팹을 연결해주세요.",
                this
            );

            Destroy(gameObject);
            yield break;
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, followUpDelay)
        );

        // 후속 발사 시점에 살아 있는 주변 적을 찾습니다.
        enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        List<EnemyHealth> candidates = new();
        float searchSquared = searchRadius * searchRadius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsAlive(enemy))
                continue;

            Vector2 offset = enemy.transform.position - center;

            if (offset.sqrMagnitude <= searchSquared)
                candidates.Add(enemy);
        }

        candidates.Sort((a, b) =>
        {
            float distanceA =
                ((Vector2)(a.transform.position - center)).sqrMagnitude;

            float distanceB =
                ((Vector2)(b.transform.position - center)).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        int count = Mathf.Min(dropletCount, candidates.Count);

        // 가까운 서로 다른 적에게 동시에 발사합니다.
        for (int i = 0; i < count; i++)
        {
            SlimeProjectile droplet = Instantiate(
                dropletPrefab,
                center,
                Quaternion.identity
            );

            droplet.Initialize(
                candidates[i],
                attackDamage * dropletMultiplier,
                color
            );
        }

        // 발사된 물방울은 독립적으로 이동합니다.
        Destroy(gameObject);
    }

    private static bool IsAlive(EnemyHealth enemy)
    {
        return enemy != null &&
               !enemy.IsDead &&
               enemy.isActiveAndEnabled;
    }
}
