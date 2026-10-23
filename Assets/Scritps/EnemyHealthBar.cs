using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth health;
    [SerializeField] private Transform fill;

    [SerializeField] private bool hideWhenFull = true;

    private SpriteRenderer[] barRenderers;

    private Vector3 fullScale;
    private Vector3 fullPosition;

    private void Awake()
    {
        if (health == null)
            health = GetComponentInParent<EnemyHealth>();

        if (health == null || fill == null)
        {
            Debug.LogError(
                "체력바의 EnemyHealth와 Fill을 연결해주세요.",
                this
            );

            enabled = false;
            return;
        }

        fullScale = fill.localScale;
        fullPosition = fill.localPosition;

        barRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (health == null)
            return;

        float ratio = Mathf.Clamp01(
            health.CurrentHealth / Mathf.Max(1f, health.MaxHealth)
        );

        Vector3 scale = fullScale;
        scale.x *= ratio;
        fill.localScale = scale;

        Vector3 position = fullPosition;
        position.x -= fullScale.x * (1f - ratio) * 0.5f;
        fill.localPosition = position;

        bool visible =
            !health.IsDead &&
            (!hideWhenFull || ratio < 0.9999f);

        foreach (SpriteRenderer renderer in barRenderers)
            renderer.enabled = visible;
    }
}
