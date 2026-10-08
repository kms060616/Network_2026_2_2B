using TMPro;
using UnityEngine;
using System.Collections;

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

        for (int wave = 1; wave <= Mathf.Max(1, totalWaves); wave++)
        {
            currentWave = wave;

            float health = firstWaveHealth +
                (wave - 1) * healthGrowthPerWave;

            int count = Mathf.Max(1, enemiesPerWave);
            pendingSpawns += count;

            StartCoroutine(SpawnWave(health, count));

            if (wave < totalWaves)
            {
                nextWaveTime = Time.time +
                    Mathf.Max(0.1f, waveInterval);

                yield return new WaitForSeconds(
                    Mathf.Max(0.1f, waveInterval)
                );
            }
        }

        if (statusText != null)
            statusText.text = "남은 적을 모두 처치하세요.";

        // 마지막 웨이브의 생성과 처치가 모두 끝나야 클리어합니다.
        while (pendingSpawns > 0 || EnemyHealth.AliveCount > 0)
            yield return null;

        EndGame(true);
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

    private void EndGame(bool cleared)
    {
        if (gameEnded)
            return;

        gameEnded = true;
        StopAllCoroutines();

        if (statusText != null)
        {
            statusText.text = cleared
                ? "테스트 클리어!"
                : $"패배! 적이 {enemyLimit}마리를 넘었습니다.";
        }

        Debug.Log(cleared ? "테스트 클리어" : "패배", this);

        // 적 이동과 공격을 정지합니다.
        Time.timeScale = 0f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
