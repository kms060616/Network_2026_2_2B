using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DispatchManager : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private BoardInput boardInput;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private SlimeSummoner summoner;
    [SerializeField] private TMP_Text dispatchText;

    [Header("미니보스")]
    [SerializeField, Min(1f)] private float firstBossHealth = 300f;
    [SerializeField, Min(1f)] private float healthGrowth = 1.5f;
    [SerializeField, Min(0)] private int rewardGold = 100;
    [SerializeField, Min(0.1f)] private float respawnDelay = 15f;

    private readonly List<SlimeUnit> dispatchedUnits = new();

    // 중도 복귀 후 다시 파견해도 공격 대기시간을 유지합니다.
    private readonly Dictionary<SlimeUnit, float> nextAttackTimes = new();

    private float bossMaxHealth;
    private float bossHealth;
    private float respawnRemaining;

    private int defeatedCount;
    private bool bossActive;

    public bool IsBossActive => bossActive;
    public float BossHealth => bossHealth;
    public float BossMaxHealth => bossMaxHealth;

    public IReadOnlyList<SlimeUnit> DispatchedUnits =>
        dispatchedUnits;

    private void Awake()
    {
        bossMaxHealth = Mathf.Max(1f, firstBossHealth);
        SpawnMiniBoss();
    }

    private bool CanOperate()
    {
        return boardInput != null &&
               boardInput.isActiveAndEnabled &&
               waveManager != null &&
               !waveManager.IsGameEnded &&
               summoner != null;
    }

    public void DispatchSelected()
    {
        if (!CanOperate())
            return;

        if (!bossActive)
        {
            Debug.Log("미니보스가 재등장하면 파견할 수 있습니다.", this);
            return;
        }

        SlimeUnit unit = boardInput.SelectedUnit;

        if (unit == null || !unit.TryDispatch())
            return;

        dispatchedUnits.Add(unit);

        if (!nextAttackTimes.ContainsKey(unit))
            nextAttackTimes.Add(unit, Time.time);

        boardInput.ClearSelection();
        UpdateText();
    }

    public void ReturnAll()
    {
        if (!CanOperate())
            return;

        ReturnAllInternal();
        UpdateText();
    }

    private void ReturnAllInternal()
    {
        for (int i = dispatchedUnits.Count - 1; i >= 0; i--)
        {
            SlimeUnit unit = dispatchedUnits[i];

            if (unit == null || unit.TryReturnFromDispatch())
                dispatchedUnits.RemoveAt(i);
        }
    }

    private void Update()
    {
        if (waveManager == null ||
            waveManager.IsGameEnded ||
            Time.timeScale == 0f)
        {
            return;
        }

        dispatchedUnits.RemoveAll(
            unit => unit == null || !unit.IsDispatched
        );

        if (!bossActive)
        {
            respawnRemaining = Mathf.Max(
                0f,
                respawnRemaining - Time.deltaTime
            );

            if (respawnRemaining <= 0f)
                SpawnMiniBoss();

            return;
        }

        for (int i = 0; i < dispatchedUnits.Count; i++)
        {
            SlimeUnit unit = dispatchedUnits[i];

            if (!unit.isActiveAndEnabled || unit.Data == null)
                continue;

            if (!nextAttackTimes.TryGetValue(unit, out float nextTime))
                nextTime = Time.time;

            if (Time.time < nextTime)
                continue;

            nextAttackTimes[unit] = Time.time + unit.AttackInterval;

            float damage = unit.AttackDamage;
            SynergyManager synergy = SynergyManager.Instance;

            if (synergy != null)
            {
                float chance = synergy.GetCriticalChance(
                    unit.Data.element
                );

                if (chance > 0f && Random.value < chance)
                    damage *= synergy.GetCriticalMultiplier();
            }

            bossHealth = Mathf.Max(0f, bossHealth - damage);

            if (bossHealth <= 0f)
            {
                DefeatMiniBoss();
                break;
            }
        }
    }

    private void SpawnMiniBoss()
    {
        bossHealth = bossMaxHealth;
        bossActive = true;
        respawnRemaining = 0f;
        UpdateText();
    }

    private void DefeatMiniBoss()
    {
        // 보상을 중복 지급하지 않도록 먼저 비활성 상태로 변경합니다.
        if (!bossActive)
            return;

        bossActive = false;
        defeatedCount++;

        summoner.GrantGold(rewardGold);
        ReturnAllInternal();

        bossMaxHealth = Mathf.Min(
            100000000f,
            bossMaxHealth * Mathf.Max(1f, healthGrowth)
        );

        respawnRemaining = Mathf.Max(0.1f, respawnDelay);

        Debug.Log(
            $"미니보스 처치! 보상 {rewardGold}G, 자동 복귀",
            this
        );

        UpdateText();
    }

    private void LateUpdate()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (dispatchText == null)
            return;

        string text = bossActive
            ? $"미니보스 체력: {bossHealth:0.#}/{bossMaxHealth:0.#}"
            : $"재등장: {Mathf.CeilToInt(respawnRemaining)}초";

        text += $"\n처치 보상: {rewardGold}G";
        text += $"\n누적 처치: {defeatedCount}";
        text += $"\n파견 중: {dispatchedUnits.Count}마리";

        foreach (SlimeUnit unit in dispatchedUnits)
        {
            if (unit != null && unit.Data != null)
                text += $"\n{unit.Data.unitName}";
        }

        dispatchText.text = text;
    }
}
