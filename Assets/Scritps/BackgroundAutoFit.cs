using UnityEngine;



[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundAutoFit : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Tooltip("가장자리 틈을 방지하기 위한 추가 확대 비율")]
    [SerializeField, Range(0f, 0.1f)]
    private float extraCoverage = 0.01f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null || !targetCamera.orthographic)
        {
            Debug.LogError(
                "배경 자동 맞춤에는 Orthographic 카메라가 필요합니다.",
                this
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        FitBackground();
    }

    private void FitBackground()
    {
        if (targetCamera == null || spriteRenderer.sprite == null)
            return;

        Bounds bounds = spriteRenderer.sprite.bounds;

        if (bounds.size.x <= 0f || bounds.size.y <= 0f)
            return;

        float screenHeight = targetCamera.orthographicSize * 2f;
        float screenWidth = screenHeight * targetCamera.aspect;

        // 가로·세로 중 더 큰 배율을 사용해 화면 전체를 덮습니다.
        float scale = Mathf.Max(
            screenWidth / bounds.size.x,
            screenHeight / bounds.size.y
        );

        scale *= 1f + extraCoverage;

        // 이 오브젝트는 씬 최상위에 두고 사용합니다.
        transform.localScale = new Vector3(scale, scale, 1f);

        // Sprite의 Pivot이 중앙이 아니어도 화면 가운데에 맞춥니다.
        Vector3 position = transform.position;

        position.x = targetCamera.transform.position.x -
            bounds.center.x * scale;

        position.y = targetCamera.transform.position.y -
            bounds.center.y * scale;

        transform.position = position;
    }
}
