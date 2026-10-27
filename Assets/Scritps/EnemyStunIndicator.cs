using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class EnemyStunIndicator : MonoBehaviour
{
    [SerializeField] private EnemyStun stun;
    [SerializeField] private SpriteRenderer bodyRenderer;

    [Header("표시")]
    [SerializeField, Min(0.01f)]
    private float iconWorldWidth = 0.45f;

    [SerializeField, Min(0f)]
    private float headGap = 0.08f;

    private SpriteRenderer iconRenderer;

    private void Awake()
    {
        iconRenderer = GetComponent<SpriteRenderer>();

        if (stun == null)
            stun = GetComponentInParent<EnemyStun>();

        if (bodyRenderer == null && stun != null)
            bodyRenderer = stun.GetComponent<SpriteRenderer>();

        iconRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        bool visible =
            stun != null &&
            stun.isActiveAndEnabled &&
            stun.IsStunned &&
            bodyRenderer != null &&
            bodyRenderer.enabled &&
            iconRenderer.sprite != null;

        iconRenderer.enabled = visible;

        if (!visible)
            return;

        // 부모 적의 크기와 관계없이 아이콘 너비를 일정하게 유지합니다.
        float spriteWidth = iconRenderer.sprite.bounds.size.x;

        if (spriteWidth <= 0f)
            return;

        float worldScale = iconWorldWidth / spriteWidth;

        Vector3 parentScale = transform.parent != null
            ? transform.parent.lossyScale
            : Vector3.one;

        if (Mathf.Abs(parentScale.x) < 0.0001f ||
            Mathf.Abs(parentScale.y) < 0.0001f)
        {
            return;
        }

        transform.localScale = new Vector3(
            worldScale / Mathf.Abs(parentScale.x),
            worldScale / Mathf.Abs(parentScale.y),
            1f
        );

        Bounds bodyBounds = bodyRenderer.bounds;
        Bounds iconBounds = iconRenderer.bounds;

        // 현재 아이콘의 아래쪽이 적 머리 위에 오도록 이동합니다.
        Vector3 offset = new Vector3(
            bodyBounds.center.x - iconBounds.center.x,
            bodyBounds.max.y + headGap - iconBounds.min.y,
            0f
        );

        transform.position += offset;
    }
}
