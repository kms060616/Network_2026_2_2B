using UnityEngine;


[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField]
    private Color hitColor =
        new Color(1f, 0.4f, 0.4f, 1f);

    [SerializeField, Range(0f, 1f)]
    private float intensity = 0.75f;

    [SerializeField, Min(0.01f)]
    private float flashDuration = 0.08f;

    [SerializeField, Min(0.01f)]
    private float minimumFlashInterval = 0.15f;

    private EnemyHealth health;
    private SpriteRenderer spriteRenderer;

    private Color originalColor;
    private bool flashing;
    private float elapsed;
    private float nextFlashTime;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        health.Damaged += OnDamaged;
        nextFlashTime = 0f;
    }

    private void OnDisable()
    {
        health.Damaged -= OnDamaged;

        if (flashing)
            spriteRenderer.color = originalColor;

        flashing = false;
    }

    private void OnDamaged(float damage)
    {
        if (Time.timeScale == 0f ||
            Time.time < nextFlashTime)
        {
            return;
        }

        // 웨이브에서 적용한 색을 첫 피격 때 저장합니다.
        if (!flashing)
            originalColor = spriteRenderer.color;

        flashing = true;
        elapsed = 0f;

        nextFlashTime = Time.time +
            Mathf.Max(minimumFlashInterval, flashDuration);

        ApplyFlash(1f);
    }

    private void Update()
    {
        if (!flashing || Time.timeScale == 0f)
            return;

        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(
            elapsed / Mathf.Max(0.01f, flashDuration)
        );

        ApplyFlash(1f - progress);

        if (progress >= 1f)
        {
            spriteRenderer.color = originalColor;
            flashing = false;
        }
    }

    private void ApplyFlash(float strength)
    {
        Color targetColor = hitColor;
        targetColor.a = originalColor.a;

        spriteRenderer.color = Color.Lerp(
            originalColor,
            targetColor,
            strength * intensity
        );
    }
}
