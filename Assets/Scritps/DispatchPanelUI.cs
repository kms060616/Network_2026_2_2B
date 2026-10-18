using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DispatchPanelUI : MonoBehaviour
{
    [SerializeField] private DispatchManager dispatchManager;
    [SerializeField] private WaveManager waveManager;

    [Header("패널")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image bossPortrait;
    [SerializeField] private Slider bossHealthSlider;

    [Header("파견 유닛")]
    [SerializeField] private Transform unitContainer;
    [SerializeField] private DispatchUnitIcon unitIconPrefab;

    private readonly List<SlimeUnit> displayedUnits = new();
    private readonly List<DispatchUnitIcon> icons = new();

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Open()
    {
        if (dispatchManager == null ||
            waveManager == null ||
            waveManager.IsGameEnded ||
            panel == null)
        {
            return;
        }

        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (panel == null || !panel.activeInHierarchy)
            return;

        if (waveManager == null || waveManager.IsGameEnded)
        {
            Close();
            return;
        }

        Refresh();
    }

    private void Refresh()
    {
        if (dispatchManager == null)
            return;

        bool bossActive = dispatchManager.IsBossActive;

        if (bossPortrait != null)
            bossPortrait.gameObject.SetActive(bossActive);

        if (bossHealthSlider != null)
        {
            bossHealthSlider.gameObject.SetActive(bossActive);

            bossHealthSlider.SetValueWithoutNotify(
                dispatchManager.BossHealth /
                Mathf.Max(1f, dispatchManager.BossMaxHealth)
            );
        }

        RefreshUnits();
    }

    private void RefreshUnits()
    {
        if (unitContainer == null || unitIconPrefab == null)
            return;

        IReadOnlyList<SlimeUnit> units =
            dispatchManager.DispatchedUnits;

        bool changed = displayedUnits.Count != units.Count;

        if (!changed)
        {
            for (int i = 0; i < units.Count; i++)
            {
                if (displayedUnits[i] != units[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (!changed)
            return;

        foreach (DispatchUnitIcon icon in icons)
        {
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
                Destroy(icon.gameObject);
            }
        }

        icons.Clear();
        displayedUnits.Clear();

        foreach (SlimeUnit unit in units)
        {
            DispatchUnitIcon icon = Instantiate(
                unitIconPrefab,
                unitContainer
            );

            icon.Bind(unit);

            icons.Add(icon);
            displayedUnits.Add(unit);
        }
    }
}
