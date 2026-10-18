using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DispatchUnitIcon : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text nameText;

    public void Bind(SlimeUnit unit)
    {
        if (unit == null || unit.Data == null)
            return;

        SlimeData data = unit.Data;

        if (portrait != null)
        {
            Sprite sprite = data.sprite;

            // 데이터에 이미지가 없으면 기존 유닛의 Sprite 사용
            if (sprite == null &&
                unit.TryGetComponent(out SpriteRenderer renderer))
            {
                sprite = renderer.sprite;
            }

            portrait.sprite = sprite;
            portrait.color = data.color;
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
        }

        if (nameText != null)
        {
            string upgrade = unit.UpgradeLevel > 0
                ? $" +{unit.UpgradeLevel}"
                : "";

            nameText.text = data.unitName + upgrade;
            nameText.raycastTarget = false;
        }
    }
}
