using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class SlimeIdleAnimation : MonoBehaviour
{
    [Header("대기 움직임")]
    [SerializeField, Range(0f, 0.15f)]
    private float amount = 0.04f;

    [SerializeField, Min(0.1f)]
    private float cycleDuration = 1.2f;

    [Header("공격 움직임")]
    [SerializeField, Range(0f, 0.3f)]
    private float attackStretch = 0.14f;

    [SerializeField, Min(0.01f)]
    private float attackDuration = 0.2f;

    private SpriteRenderer spriteRenderer;
    private SlimeUnit unit;

    private Vector3 originalScale;
    private Vector3 originalPosition;

    private float phase;
    private bool attacking;
    private float attackElapsed;
    private float currentAttackDuration;
    private float attackStartDeformation;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        unit = GetComponentInParent<SlimeUnit>();

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        phase = Random.Range(0f, Mathf.PI * 2f);
    }

    public void PlayAttack(float attackInterval)
    {
        if (!isActiveAndEnabled ||
            !spriteRenderer.enabled ||
            Time.timeScale == 0f ||
            (unit != null && unit.IsDispatched))
        {
            return;
        }

        // 현재 모양에서 시작해 갑자기 크기가 튀지 않게 합니다.
        attackStartDeformation = originalScale.y != 0f
            ? transform.localScale.y / originalScale.y - 1f
            : 0f;

        attacking = true;
        attackElapsed = 0f;

        // 빠른 공격에서는 애니메이션도 짧게 재생합니다.
        currentAttackDuration = Mathf.Min(
            Mathf.Max(0.01f, attackDuration),
            Mathf.Max(0.01f, attackInterval) * 0.8f
        );
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f ||
            spriteRenderer.sprite == null ||
            !spriteRenderer.enabled ||
            (unit != null && unit.IsDispatched))
        {
            return;
        }

        phase += Time.deltaTime *
            Mathf.PI * 2f /
            Mathf.Max(0.1f, cycleDuration);

        phase = Mathf.Repeat(phase, Mathf.PI * 2f);

        float idleDeformation = Mathf.Sin(phase) * amount;
        float deformation = idleDeformation;

        if (attacking)
        {
            attackElapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                attackElapsed / currentAttackDuration
            );

            if (t < 0.3f)
            {
                // 공격 순간 위로 늘어납니다.
                deformation = SmoothBetween(
                    attackStartDeformation,
                    attackStretch,
                    t / 0.3f
                );
            }
            else if (t < 0.65f)
            {
                // 반동으로 살짝 눌립니다.
                deformation = SmoothBetween(
                    attackStretch,
                    -attackStretch * 0.5f,
                    (t - 0.3f) / 0.35f
                );
            }
            else
            {
                // 대기 움직임으로 복귀합니다.
                deformation = SmoothBetween(
                    -attackStretch * 0.5f,
                    idleDeformation,
                    (t - 0.65f) / 0.35f
                );
            }

            if (t >= 1f)
                attacking = false;
        }

        ApplyDeformation(deformation);
    }

    private static float SmoothBetween(float from, float to, float t)
    {
        return Mathf.Lerp(
            from,
            to,
            Mathf.SmoothStep(0f, 1f, t)
        );
    }

    private void ApplyDeformation(float deformation)
    {
        Vector3 scale = originalScale;
        scale.x *= 1f - deformation;
        scale.y *= 1f + deformation;

        transform.localScale = scale;

        float bottom = spriteRenderer.sprite.bounds.min.y;

        Vector3 position = originalPosition;
        position.y += bottom * (originalScale.y - scale.y);

        transform.localPosition = position;
    }

    private void OnDisable()
    {
        attacking = false;
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
    }
}
