using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyMovement enemyPrefab;
    [SerializeField] private Transform[] points;
    [SerializeField, Min(0.1f)] private float spawnInterval = 2f;
    [SerializeField, Min(1)] private int spawnCount = 10;

    private IEnumerator Start()
    {
        if (enemyPrefab == null || points == null || points.Length < 2)
        {
            Debug.LogError("적 프리팹과 경로 지점을 연결해주세요.", this);
            yield break;
        }

        foreach (Transform point in points)
        {
            if (point == null)
            {
                Debug.LogError("비어 있는 경로 지점이 있습니다.", this);
                yield break;
            }
        }

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyMovement enemy = Instantiate(
                enemyPrefab,
                points[0].position,
                Quaternion.identity
            );

            // EnemyMovement의 Start가 실행되기 전에 경로를 전달합니다.
            enemy.SetPath(points);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
