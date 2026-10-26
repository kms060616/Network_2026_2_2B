using UnityEngine;


[RequireComponent(typeof(LineRenderer))]
public class LightningBolt : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float duration = 0.15f;

    [SerializeField, Min(0.001f)]
    private float width = 0.045f;

    [SerializeField, Range(3, 20)]
    private int pointCount = 8;

    [SerializeField, Min(0f)]
    private float zigzagAmount = 0.12f;

    private LineRenderer line;
    private Color color;
    private float elapsed;
    private bool initialized;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        line.useWorldSpace = true;
        line.loop = false;
        line.startWidth = width;
        line.endWidth = width;
        line.enabled = false;
    }

    public void Initialize(Vector3 start, Vector3 end, Color boltColor)
    {
        color = boltColor;
        elapsed = 0f;

        int count = Mathf.Max(3, pointCount);
        line.positionCount = count;

        Vector3 difference = end - start;

        Vector3 perpendicular = new Vector3(
            -difference.y,
            difference.x,
            0f
        ).normalized;

        float jitter = Mathf.Min(
            zigzagAmount,
            difference.magnitude * 0.2f
        );

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);
            Vector3 position = Vector3.Lerp(start, end, t);

            // 양 끝은 정확한 시작점과 명중 위치를 유지합니다.
            if (i > 0 && i < count - 1)
            {
                position += perpendicular *
                    Random.Range(-jitter, jitter);
            }

            line.SetPosition(i, position);
        }

        line.startColor = color;
        line.endColor = color;
        line.enabled = true;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || Time.timeScale == 0f)
            return;

        elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(
            elapsed / Mathf.Max(0.01f, duration)
        );

        Color faded = color;
        faded.a *= 1f - progress;

        line.startColor = faded;
        line.endColor = faded;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
