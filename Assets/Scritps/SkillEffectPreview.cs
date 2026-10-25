using UnityEngine;
using UnityEngine.UI;

public class SkillEffectPreview : MonoBehaviour
{
    [SerializeField] private BoardInput boardInput;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Button previewButton;

    [Header("테스트 표시")]
    [SerializeField]
    private Vector3 positionOffset =
        new Vector3(0f, 0.8f, 0f);

    [SerializeField] private bool useBrightColor = true;

    [SerializeField]
    private Color previewColor =
        new Color(1f, 0.9f, 0.1f, 1f);

    private SlimeSkillEffect currentPreview;

    private SlimeUnit GetSelectedUnit()
    {
        if (boardInput == null ||
            !boardInput.isActiveAndEnabled ||
            waveManager == null ||
            waveManager.IsGameEnded ||
            Time.timeScale == 0f)
        {
            return null;
        }

        SlimeUnit unit = boardInput.SelectedUnit;

        if (unit == null ||
            !unit.isActiveAndEnabled ||
            unit.IsDispatched ||
            unit.Data == null ||
            unit.Data.skill == null ||
            unit.Data.skill.effectPrefab == null)
        {
            return null;
        }

        return unit;
    }

    private void LateUpdate()
    {
        if (previewButton != null)
            previewButton.interactable = GetSelectedUnit() != null;
    }

    public void PreviewSelected()
    {
        SlimeUnit unit = GetSelectedUnit();

        if (unit == null)
            return;

        // 반복 클릭해도 이전 테스트 효과와 겹치지 않게 합니다.
        if (currentPreview != null)
        {
            currentPreview.gameObject.SetActive(false);
            Destroy(currentPreview.gameObject);
        }

        SlimeSkillData skill = unit.Data.skill;

        currentPreview = Instantiate(
            skill.effectPrefab,
            unit.transform.position + positionOffset,
            Quaternion.identity
        );

        currentPreview.SetPreviewSettings();

        Color color = useBrightColor
            ? previewColor
            : skill.effectColor;

        currentPreview.Initialize(
            Mathf.Max(0.5f, skill.effectRadius),
            color
        );
    }
}
