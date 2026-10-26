using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ChainLightningSkill : MonoBehaviour
{
    [SerializeField] private LightningBolt boltPrefab;

    private readonly HashSet<EnemyHealth> visited = new();

    public void Initialize(
        Vector3 origin,
        EnemyHealth firstTarget,
        float firstDamage,
        int maximumTargets,
        float jumpRange,
        float damageRetention,
        float jumpDelay,
        Color color)
    {
        StartCoroutine(Execute(
            origin,
            firstTarget,
            firstDamage,
            maximumTargets,
            jumpRange,
            damageRetention,
            jumpDelay,
            color
        ));
    }

    private IEnumerator Execute(
        Vector3 origin,
        EnemyHealth target,
        float damage,
        int maximumTargets,
        float jumpRange,
        float damageRetention,
        float jumpDelay,
        Color color)
    {
        visited.Clear();

        Vector3 previousHit = origin;
        bool hasHit = false;

        for (int i = 0; i < Mathf.Max(1, maximumTargets); i++)
        {
            // 연결 대기 중 대상이 죽었다면 이전 명중점에서 다시 찾습니다.
            if (!IsValid(target))
            {
                if (!hasHit)
                    break;

                target = FindNext(previousHit, jumpRange);
            }

            if (!IsValid(target))
                break;

            Vector3 hitPosition = target.transform.position;
            visited.Add(target);

            if (boltPrefab != null)
            {
                LightningBolt bolt = Instantiate(boltPrefab);

                bolt.Initialize(
                    previousHit,
                    hitPosition,
                    color
                );
            }

            target.TakeDamage(damage);

            previousHit = hitPosition;
            hasHit = true;
            damage *= Mathf.Clamp01(damageRetention);

            if (i >= maximumTargets - 1 || damage <= 0f)
                break;

            yield return new WaitForSeconds(
                Mathf.Max(0.01f, jumpDelay)
            );

            target = FindNext(previousHit, jumpRange);
        }

        Destroy(gameObject);
    }

    private bool IsValid(EnemyHealth enemy)
    {
        return enemy != null &&
               !enemy.IsDead &&
               enemy.isActiveAndEnabled &&
               !visited.Contains(enemy);
    }

    private EnemyHealth FindNext(Vector3 center, float range)
    {
        EnemyHealth nearest = null;
        float bestDistance = range * range;

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            if (!IsValid(enemy))
                continue;

            Vector2 offset = enemy.transform.position - center;
            float distance = offset.sqrMagnitude;

            if (distance <= bestDistance)
            {
                nearest = enemy;
                bestDistance = distance;
            }
        }

        return nearest;
    }
}
