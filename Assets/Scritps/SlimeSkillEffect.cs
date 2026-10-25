using UnityEngine;



[RequireComponent(typeof(LineRenderer))]
public class SlimeSkillEffect : MonoBehaviour
{
    [Header("파동")]
    [SerializeField, Min(0.01f)]
    private float duration = 0.6f;

    [SerializeField, Min(0.001f)]
    private float lineWidth = 0.05f;

    [SerializeField, Range(16, 128)]
    private int segments = 48;

    [Header("입자 크기")]
    [SerializeField, Min(0.001f)]
    private float minimumParticleSize = 0.06f;

    [SerializeField, Min(0.001f)]
    private float maximumParticleSize = 0.1f;

    [Header("입자")]
    [SerializeField] private ParticleSystem burstParticles;

    [SerializeField, Range(1, 32)]
    private int particleCount = 14;

    private LineRenderer firstRing;
    private LineRenderer secondRing;

    private Color effectColor;
    private float radius;
    private float elapsed;
    private bool initialized;

    private Vector3[] positions;

    private void Awake()
    {
        segments = Mathf.Max(16, segments);
        positions = new Vector3[segments];

        firstRing = GetComponent<LineRenderer>();
        SetupRing(firstRing);

        GameObject secondObject = new GameObject("SecondRing");
        secondObject.transform.SetParent(transform, false);

        secondRing = secondObject.AddComponent<LineRenderer>();
        secondRing.sharedMaterial = firstRing.sharedMaterial;
        secondRing.sortingLayerID = firstRing.sortingLayerID;
        secondRing.sortingOrder = firstRing.sortingOrder;

        SetupRing(secondRing);
    }

    private void SetupRing(LineRenderer ring)
    {
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = segments;
        ring.enabled = false;
        ring.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
    }

    public void Initialize(float effectRadius, Color color)
    {
        radius = Mathf.Max(0f, effectRadius);
        effectColor = color;
        elapsed = 0f;
        initialized = true;

        Draw(0f);
        EmitParticles();
    }

    private void Update()
    {
        if (!initialized || Time.timeScale == 0f)
            return;

        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(
            elapsed / Mathf.Max(0.01f, duration)
        );

        Draw(progress);

        // 입자가 남아 있는 동안에는 삭제하지 않습니다.
        if (progress >= 1f)
        {
            if (burstParticles != null)
            {
                burstParticles.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void Draw(float progress)
    {
        DrawRing(
            firstRing,
            progress,
            0.25f,
            1f,
            1f
        );

        // 두 번째 파동은 조금 늦게 표시합니다.
        float secondProgress = (progress - 0.18f) / 0.82f;

        if (secondProgress < 0f)
        {
            secondRing.enabled = false;
        }
        else
        {
            DrawRing(
                secondRing,
                Mathf.Clamp01(secondProgress),
                0.2f,
                0.85f,
                0.65f
            );
        }
    }

    private void DrawRing(
        LineRenderer ring,
        float progress,
        float startRadius,
        float endRadius,
        float strength)
    {
        ring.enabled = progress < 1f;

        if (!ring.enabled)
            return;

        float currentRadius = radius *
            Mathf.Lerp(startRadius, endRadius, progress);

        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            positions[i] = new Vector3(
                Mathf.Cos(angle) * currentRadius,
                Mathf.Sin(angle) * currentRadius,
                0f
            );
        }

        ring.SetPositions(positions);

        float width = lineWidth *
            Mathf.Lerp(1f, 0.2f, progress) * strength;

        ring.startWidth = width;
        ring.endWidth = width;

        Color color = effectColor;
        color.a *= (1f - progress) * strength;

        ring.startColor = color;
        ring.endColor = color;
    }

    private void EmitParticles()
    {
        if (burstParticles == null)
            return;

        burstParticles.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        burstParticles.Play();

        for (int i = 0; i < particleCount; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float lifetime = Random.Range(0.5f, 0.8f);

            Vector3 direction = new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f
            );

            float travelDistance =
                radius * Random.Range(0.35f, 0.8f);

            ParticleSystem.EmitParams particle =
                new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = direction *
                        (travelDistance / lifetime),
                    startLifetime = lifetime,
                    startSize = Random.Range(
    minimumParticleSize,
    Mathf.Max(minimumParticleSize, maximumParticleSize)
),
                    startColor = effectColor
                };

            burstParticles.Emit(particle, 1);
        }
    }

    public void SetPreviewSettings()
    {
        // 생성된 테스트 효과에만 적용합니다.
        duration = 0.5f;
        lineWidth = 0.08f;
        minimumParticleSize = 0.12f;
        maximumParticleSize = 0.2f;
        particleCount = 12;
    }
}
