using UnityEngine;

public class MeteorImpact : MonoBehaviour
{
    [SerializeField]
    private Vector3 fallOffset =
        new Vector3(2f, 5f, 0f);

    private Vector3 startPosition;
    private Vector3 impactPosition;

    private float damage;
    private float radius;
    private float stunDuration;
    private float fallDuration;
    private float elapsed;

    private SlimeSkillEffect impactEffect;
    private Color effectColor;
    private bool initialized;

    public void Initialize(
        Vector3 center,
        float hitDamage,
        float hitRadius,
        float stunTime,
        float fallTime,
        SlimeSkillEffect effect,
        Color color)
    {
        impactPosition = center;
        startPosition = center + fallOffset;

        damage = hitDamage;
        radius = Mathf.Max(0f, hitRadius);
        stunDuration = Mathf.Max(0f, stunTime);
        fallDuration = Mathf.Max(0.01f, fallTime);

        impactEffect = effect;
        effectColor = color;

        transform.position = startPosition;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || Time.timeScale == 0f)
            return;

        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(elapsed / fallDuration);

        // 처음에는 느리고, 지면에 가까워질수록 빨라집니다.
        transform.position = Vector3.Lerp(
            startPosition,
            impactPosition,
            progress * progress
        );

        if (progress >= 1f)
            Impact();
    }

    private void Impact()
    {
        initialized = false;

        if (impactEffect != null)
        {
            SlimeSkillEffect effect = Instantiate(
                impactEffect,
                impactPosition,
                Quaternion.identity
            );

            effect.Initialize(radius, effectColor);
        }

        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        float radiusSquared = radius * radius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy.IsDead || !enemy.isActiveAndEnabled)
                continue;

            Vector2 offset =
                enemy.transform.position - impactPosition;

            if (offset.sqrMagnitude > radiusSquared)
                continue;

            enemy.TakeDamage(damage);

            // 피해를 받고 살아남은 적에게만 스턴을 적용합니다.
            if (!enemy.IsDead &&
                enemy.TryGetComponent(out EnemyStun stun))
            {
                stun.ApplyStun(stunDuration);
            }
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
