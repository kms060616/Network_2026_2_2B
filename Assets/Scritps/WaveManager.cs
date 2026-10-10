using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("적 생성")]
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private Transform[] points;

    [Header("웨이브")]
    [SerializeField, Min(1)] private int totalWaves = 10;
    [SerializeField, Min(0f)] private float preparationTime = 10f;
    [SerializeField, Min(0.1f)] private float waveInterval = 25f;
    [SerializeField, Min(0.1f)] private float spawnInterval = 1f;
    [SerializeField, Min(1)] private int enemiesPerWave = 10;

    [Header("적 능력치")]
    [SerializeField, Min(1f)] private float firstWaveHealth = 30f;
    [SerializeField, Min(0f)] private float healthGrowthPerWave = 10f;
    [SerializeField, Min(0)] private int goldReward = 10;

    [Header("패배 조건")]
    [SerializeField, Min(1)] private int enemyLimit = 70;

    [Header("화면 표시")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemyCountText;
    [SerializeField] private TMP_Text statusText;

    [Header("보스")]
    [SerializeField] private EnemyMovement bossPrefab;
    [SerializeField, Min(1)] private int bossEveryWaves = 10;
    [SerializeField, Min(0.1f)] private float bossTimeLimit = 180f;
    [SerializeField, Min(1f)] private float firstBossHealth = 300f;
    [SerializeField, Min(0f)] private float bossHealthGrowth = 200f;
    [SerializeField, Min(0)] private int bossGoldReward = 100;
    [SerializeField] private TMP_Text bossText;

    [Header("결과 화면")]
    [SerializeField] private ResultUI resultUI;

    public bool IsGameEnded => gameEnded;

    private float sessionStartTime;

    private readonly List<BossTimer> bosses = new();

    private int currentWave;
    private int pendingSpawns;
    private float nextWaveTime;
    private bool ready;
    private bool gameEnded;

    private void Start()
    {
        Time.timeScale = 1f;

        if (!ValidateSetup())
        {
            if (statusText != null)
                statusText.text = "웨이브 설정을 확인해주세요.";

            return;
        }

        ready = true;
        StartCoroutine(RunWaves());

        Time.timeScale = 1f;
        sessionStartTime = Time.time;
    }

    private bool ValidateSetup()
    {
        if (enemyPrefab == null ||
            points == null || points.Length < 2)
        {
            Debug.LogError("적 프리팹과 경로를 연결해주세요.", this);
            return false;
        }

        if (enemyPrefab.GetComponent<EnemyHealth>() == null)
        {
            Debug.LogError("적 프리팹에 EnemyHealth가 필요합니다.", this);
            return false;
        }

        foreach (Transform point in points)
        {
            if (point == null)
            {
                Debug.LogError("비어 있는 경로 지점이 있습니다.", this);
                return false;
            }
        }

        if (bossPrefab == null ||
    bossPrefab.GetComponent<EnemyHealth>() == null ||
    bossPrefab.GetComponent<BossTimer>() == null)
        {
            Debug.LogError(
                "Boss 프리팹에 EnemyHealth와 BossTimer를 연결해주세요.",
                this
            );
            return false;
        }

        return true;
    }

    private void Update()
    {
        if (!ready)
            return;

        if (enemyCountText != null)
        {
            enemyCountText.text =
                $"남은 적: {EnemyHealth.AliveCount} / {enemyLimit}";
        }

        if (gameEnded)
            return;

        // 70마리는 허용, 71마리부터 패배합니다.
        if (EnemyHealth.AliveCount > enemyLimit)
        {
            EndGame(false);
            return;
        }

        if (waveText != null)
        {
            if (currentWave == 0)
            {
                waveText.text =
                    $"준비: {RemainingSeconds()}초";
            }
            else if (currentWave < totalWaves)
            {
                waveText.text =
                    $"웨이브 {currentWave}/{totalWaves}" +
                    $" · 다음: {RemainingSeconds()}초";
            }
            else
            {
                waveText.text =
                    $"웨이브 {currentWave}/{totalWaves}";
            }
        }
        if (!ready)
            return;
        UpdateBossText();
    }



    private int RemainingSeconds()
    {
        return Mathf.CeilToInt(
            Mathf.Max(0f, nextWaveTime - Time.time)
        );
    }

    private IEnumerator RunWaves()
    {
        nextWaveTime = Time.time + Mathf.Max(0f, preparationTime);

        yield return new WaitForSeconds(
            Mathf.Max(0f, preparationTime)
        );

        int waveCount = Mathf.Max(1, totalWaves);
        int bossInterval = Mathf.Max(1, bossEveryWaves);

        for (int wave = 1; wave <= waveCount; wave++)
        {
            currentWave = wave;

            // 보스 웨이브에도 일반 적은 등장합니다.
            if (wave % bossInterval == 0)
            {
                int bossNumber = wave / bossInterval;
                SpawnBoss(bossNumber);

                if (gameEnded)
                    yield break;
            }

            float health = firstWaveHealth +
                (wave - 1) * healthGrowthPerWave;

            int count = Mathf.Max(1, enemiesPerWave);
            pendingSpawns += count;

            StartCoroutine(SpawnWave(health, count));

            if (gameEnded)
                yield break;

            if (wave < waveCount)
            {
                nextWaveTime = Time.time +
                    Mathf.Max(0.1f, waveInterval);

                yield return new WaitForSeconds(
                    Mathf.Max(0.1f, waveInterval)
                );
            }
        }

        if (statusText != null)
            statusText.text = "보스와 남은 적을 모두 처치하세요.";

        while (pendingSpawns > 0 || EnemyHealth.AliveCount > 0)
            yield return null;

        EndGame(true);
    }

    private void SpawnBoss(int bossNumber)
    {
        EnemyMovement boss = Instantiate(
            bossPrefab,
            points[0].position,
            Quaternion.identity
        );

        boss.name = $"Boss_{currentWave}";
        boss.SetPath(points);

        float health = firstBossHealth +
            (bossNumber - 1) * bossHealthGrowth;

        boss.GetComponent<EnemyHealth>().Configure(
            health,
            bossGoldReward
        );

        BossTimer timer = boss.GetComponent<BossTimer>();

        int appearedWave = currentWave;

        timer.Initialize(
            bossTimeLimit,
            () => EndGame(
                false,
                $"{appearedWave}웨이브 보스 제한 시간 초과!"
            )
        );

        bosses.Add(timer);

        if (EnemyHealth.AliveCount > enemyLimit)
            EndGame(false);
    }

    private void UpdateBossText()
    {
        bosses.RemoveAll(boss => boss == null || !boss.IsAlive);

        if (bossText == null)
            return;

        string text = "";

        foreach (BossTimer boss in bosses)
        {
            int seconds = Mathf.CeilToInt(boss.RemainingTime);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            if (text.Length > 0)
                text += "\n";

            text += $"{boss.name}: {minutes}:{remainingSeconds:00}";
        }

        bossText.text = text;
    }

    private IEnumerator SpawnWave(float health, int count)
    {
        for (int i = 0; i < count; i++)
        {
            EnemyMovement enemy = Instantiate(
                enemyPrefab,
                points[0].position,
                Quaternion.identity
            );

            enemy.SetPath(points);

            EnemyHealth enemyHealth =
                enemy.GetComponent<EnemyHealth>();

            enemyHealth.Configure(health, goldReward);
            pendingSpawns--;

            if (EnemyHealth.AliveCount > enemyLimit)
            {
                EndGame(false);
                yield break;
            }

            if (i < count - 1)
            {
                yield return new WaitForSeconds(
                    Mathf.Max(0.1f, spawnInterval)
                );
            }
        }
    }

    private void EndGame(bool cleared, string reason = null)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        StopAllCoroutines();

        string message = cleared
            ? "모든 웨이브를 완료했습니다."
            : reason ?? $"적이 {enemyLimit}마리를 넘었습니다.";

        if (statusText != null)
            statusText.text = message;

        // 선택 표시를 정리하고 배치 입력을 중지합니다.
        if (BoardInput.Instance != null)
            BoardInput.Instance.enabled = false;

        if (resultUI != null)
        {
            resultUI.Show(
                cleared,
                message,
                currentWave,
                Time.time - sessionStartTime
            );
        }

        Debug.Log(message, this);

        Time.timeScale = 0f;
    }
}
