using UnityEngine;
using UnityEngine.Rendering;

public class SlimeRangeIndicator : MonoBehaviour
{
    [SerializeField] private BoardInput boardInput;

    [Header("표시")]
    [SerializeField]
    private Color fillColor =
        new Color(0.3f, 1f, 0.6f, 0.12f);

    [SerializeField]
    private Color outlineColor =
        new Color(0.3f, 1f, 0.6f, 0.8f);

    [SerializeField, Min(0.001f)]
    private float outlineWidth = 0.025f;

    [SerializeField, Range(16, 128)]
    private int segments = 64;

    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int fillSortingOrder = -1;
    [SerializeField] private int outlineSortingOrder = 1;

    private GameObject visual;
    private Mesh mesh;
    private Material material;
    private LineRenderer outline;

    private float displayedRange = -1f;

    private void Awake()
    {
        Shader shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError(
                "사거리 표시용 Sprites/Default 셰이더를 찾지 못했습니다.",
                this
            );

            enabled = false;
            return;
        }

        material = new Material(shader);
        material.color = Color.white;

        visual = new GameObject("RangeVisual");

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(visual.transform, false);

        MeshFilter filter = fillObject.AddComponent<MeshFilter>();
        MeshRenderer renderer =
            fillObject.AddComponent<MeshRenderer>();

        mesh = new Mesh();
        mesh.name = "SlimeRangeCircle";
        filter.sharedMesh = mesh;

        renderer.sharedMaterial = material;
        renderer.sortingLayerName = sortingLayerName;
        renderer.sortingOrder = fillSortingOrder;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        GameObject outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(visual.transform, false);

        outline = outlineObject.AddComponent<LineRenderer>();
        outline.sharedMaterial = material;
        outline.useWorldSpace = false;
        outline.loop = true;
        outline.startColor = outlineColor;
        outline.endColor = outlineColor;
        outline.startWidth = outlineWidth;
        outline.endWidth = outlineWidth;
        outline.sortingLayerName = sortingLayerName;
        outline.sortingOrder = outlineSortingOrder;
        outline.shadowCastingMode = ShadowCastingMode.Off;
        outline.receiveShadows = false;

        visual.SetActive(false);
    }

    private void LateUpdate()
    {
        if (visual == null)
            return;

        SlimeUnit selected = null;

        if (boardInput != null && boardInput.isActiveAndEnabled)
            selected = boardInput.SelectedUnit;

        bool visible =
            selected != null &&
            selected.isActiveAndEnabled &&
            !selected.IsDispatched &&
            selected.Data != null &&
            selected.Data.attackRange > 0f;

        visual.SetActive(visible);

        if (!visible)
            return;

        visual.transform.position = selected.transform.position;

        float range = selected.Data.attackRange;

        if (!Mathf.Approximately(range, displayedRange))
        {
            BuildCircle(range);
            displayedRange = range;
        }
    }

    private void BuildCircle(float radius)
    {
        int count = Mathf.Max(16, segments);

        Vector3[] vertices = new Vector3[count + 1];
        Color[] colors = new Color[count + 1];
        int[] triangles = new int[count * 3];

        vertices[0] = Vector3.zero;
        colors[0] = fillColor;

        outline.positionCount = count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;

            Vector3 point = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f
            );

            vertices[i + 1] = point;
            colors[i + 1] = fillColor;

            outline.SetPosition(i, point);

            int triangle = i * 3;
            triangles[triangle] = 0;
            triangles[triangle + 1] = i + 1;
            triangles[triangle + 2] = (i + 1) % count + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private void OnDisable()
    {
        if (visual != null)
            visual.SetActive(false);
    }

    private void OnDestroy()
    {
        if (visual != null)
            Destroy(visual);

        if (mesh != null)
            Destroy(mesh);

        if (material != null)
            Destroy(material);
    }
}
