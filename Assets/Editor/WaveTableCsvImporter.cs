using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WaveTableCsvImporter : MonoBehaviour
{
    private const string AssetPath =
        "Assets/Data/Waves/WaveTable.asset";

    [MenuItem("Tools/Slime Defense/Import Selected Wave CSV")]
    private static void Import()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Play 모드를 종료해주세요.");
            return;
        }

        string csvPath = AssetDatabase.GetAssetPath(
            Selection.activeObject
        );

        if (!string.Equals(
            Path.GetExtension(csvPath),
            ".csv",
            StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("Project 창에서 웨이브 CSV를 선택해주세요.");
            return;
        }

        try
        {
            // 검증이 끝난 뒤에만 에셋을 수정합니다.
            List<WaveRow> rows = ReadRows(
                File.ReadAllText(csvPath, Encoding.UTF8)
            );

            UnityEngine.Object existing =
                AssetDatabase.LoadMainAssetAtPath(AssetPath);

            if (existing != null && !(existing is WaveTable))
            {
                throw new Exception(
                    "WaveTable.asset 경로에 다른 종류의 에셋이 있습니다."
                );
            }

            WaveTable table = existing as WaveTable;

            if (table == null)
            {
                if (File.Exists(AssetPath))
                    throw new Exception("기존 WaveTable 에셋을 읽을 수 없습니다.");

                EnsureFolder("Assets/Data");
                EnsureFolder("Assets/Data/Waves");

                table = ScriptableObject.CreateInstance<WaveTable>();
                AssetDatabase.CreateAsset(table, AssetPath);
            }
            else
            {
                Undo.RecordObject(table, "Import Wave CSV");
            }

            table.waves = rows;

            foreach (WaveRow row in rows)
            {
                if (table.GetAppearance(row.appearanceId) == null)
                {
                    table.appearances.Add(new WaveAppearance
                    {
                        id = row.appearanceId
                    });
                }
            }

            EditorUtility.SetDirty(table);
            AssetDatabase.SaveAssets();

            EditorGUIUtility.PingObject(table);

            Debug.Log(
                $"웨이브 {rows.Count}개 가져오기 완료. " +
                "WaveTable의 외형 이미지와 색을 설정해주세요."
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "웨이브 가져오기 실패: " + exception.Message
            );
        }
    }

    private static List<WaveRow> ReadRows(string csv)
    {
        List<List<string>> records =
            SlimeDataCsvImporter.ParseCsv(csv);

        string[] headers =
        {
            "wave",
            "appearanceId",
            "health",
            "goldMultiplier"
        };

        if (records.Count < 2 || records[0].Count != 4)
            throw new Exception("헤더와 데이터, 총 4개 열이 필요합니다.");

        for (int i = 0; i < headers.Length; i++)
        {
            string value = records[0][i]
                .Trim()
                .TrimStart('\uFEFF');

            if (value != headers[i])
                throw new Exception($"열 이름을 확인해주세요: {headers[i]}");
        }

        List<WaveRow> rows = new();
        HashSet<int> waveNumbers = new();

        for (int i = 1; i < records.Count; i++)
        {
            List<string> fields = records[i];

            if (fields.Count != 4)
                throw new Exception($"{i + 1}번째 레코드의 열 수가 다릅니다.");

            if (!int.TryParse(fields[0].Trim(), out int wave) ||
                wave < 1 ||
                !waveNumbers.Add(wave))
            {
                throw new Exception(
                    $"{i + 1}번째 레코드의 웨이브 번호가 잘못됐거나 중복됩니다."
                );
            }

            string appearanceId = fields[1].Trim();

            if (string.IsNullOrEmpty(appearanceId))
                throw new Exception($"{wave}웨이브 외형 ID가 비어 있습니다.");

            rows.Add(new WaveRow
            {
                wave = wave,
                appearanceId = appearanceId,
                health = ReadMultiplier(fields[2], true),
                goldMultiplier = ReadMultiplier(fields[3], false)
            });
        }

        rows.Sort((a, b) => a.wave.CompareTo(b.wave));

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].wave != i + 1)
                throw new Exception("웨이브는 1부터 빠짐없이 입력해주세요.");
        }

        return rows;
    }

    private static float ReadMultiplier(string text, bool positive)
    {
        if (!float.TryParse(
                text.Trim(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float value) ||
            float.IsNaN(value) ||
            float.IsInfinity(value) ||
            value < 0f ||
            (positive && value <= 0f))
        {
            throw new Exception(
                "체력 배율은 0보다 크게, 골드 배율은 0 이상으로 입력해주세요."
            );
        }

        return value;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        int separator = path.LastIndexOf('/');

        AssetDatabase.CreateFolder(
            path.Substring(0, separator),
            path.Substring(separator + 1)
        );
    }
}
