using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// Place this file in Assets/Editor. It is not a runtime component.
public static class SlimeDataCsvImporter
{
    private const string OutputFolder = "Assets/Data/Slimes";
    private static readonly string[] Headers =
    {
        "id", "unitName", "element", "grade", "attackDamage",
        "attackInterval", "attackRange", "mergeResultId"
    };

    private sealed class Row
    {
        public string Id, Name, MergeId;
        public SlimeElement Element;
        public SlimeGrade Grade;
        public float Damage, Interval, Range;
    }

    [MenuItem("Tools/Slime Defense/Import Selected CSV")]
    private static void ImportSelectedCsv()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Import CSV", "Stop Play mode before importing.", "OK");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(sourcePath) ||
            !string.Equals(Path.GetExtension(sourcePath), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            EditorUtility.DisplayDialog("Import CSV", "Select SlimeUnits.csv in the Project window first.", "OK");
            return;
        }

        try
        {
            // Validate the entire source and resolve existing assets before writing.
            List<Row> rows = ReadRows(File.ReadAllText(sourcePath, Encoding.UTF8));
            Dictionary<string, SlimeData> assets = ResolveExistingAssets(rows);

            EnsureFolder(OutputFolder);
            int created = 0;
            int updated = 0;

            foreach (Row row in rows)
            {
                SlimeData asset = assets[row.Id];
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<SlimeData>();
                    asset.color = DefaultColor(row.Element);
                    AssetDatabase.CreateAsset(asset, OutputFolder + "/" + row.Id + ".asset");
                    assets[row.Id] = asset;
                    created++;
                }
                else
                {
                    Undo.RecordObject(asset, "Import Slime CSV");
                    updated++;
                }

                asset.unitName = row.Name;
                asset.element = row.Element;
                asset.grade = row.Grade;
                asset.attackDamage = row.Damage;
                asset.attackInterval = row.Interval;
                asset.attackRange = row.Range;
                // Existing sprite and color are intentionally preserved.
                EditorUtility.SetDirty(asset);
            }

            // Second pass: all merge targets now exist.
            foreach (Row row in rows)
            {
                SlimeData asset = assets[row.Id];
                asset.mergeResult = string.IsNullOrEmpty(row.MergeId)
                    ? null : assets[row.MergeId];
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Slime CSV import complete: {created} created, {updated} updated.");
            EditorUtility.DisplayDialog("Import complete",
                $"Created: {created}\nUpdated: {updated}\nExisting asset references were preserved.", "OK");
        }
        catch (Exception exception)
        {
            Debug.LogError("Slime CSV import failed: " + exception.Message);
            EditorUtility.DisplayDialog("Import failed", exception.Message, "OK");
        }
    }

    private static List<Row> ReadRows(string csv)
    {
        List<List<string>> records = ParseCsv(csv);
        if (records.Count < 2)
            throw new Exception("CSV must contain a header and at least one data row.");

        List<string> header = records[0];
        if (header.Count != Headers.Length)
            throw new Exception("CSV must have exactly 8 columns. Save as comma-separated CSV UTF-8.");

        for (int i = 0; i < Headers.Length; i++)
        {
            if (header[i].Trim().TrimStart('\uFEFF') != Headers[i])
                throw new Exception($"Column {i + 1} must be '{Headers[i]}'.");
        }

        var rows = new List<Row>();
        var byId = new Dictionary<string, Row>(StringComparer.OrdinalIgnoreCase);

        for (int index = 1; index < records.Count; index++)
        {
            List<string> fields = records[index];
            int rowNumber = index + 1;
            if (fields.Count != Headers.Length)
                throw new Exception($"Record {rowNumber}: expected 8 columns, found {fields.Count}.");

            for (int i = 0; i < fields.Count; i++) fields[i] = fields[i].Trim();
            string id = fields[0];
            if (!Regex.IsMatch(id, "^[a-z][a-z0-9_]*$"))
                throw new Exception($"Record {rowNumber}: invalid id '{id}'. Use lowercase letters, digits and underscores.");
            if (byId.ContainsKey(id))
                throw new Exception($"Record {rowNumber}: duplicate id '{id}'.");
            if (string.IsNullOrEmpty(fields[1]))
                throw new Exception($"Record {rowNumber}: unitName is required.");

            if (!Enum.TryParse(fields[2], false, out SlimeElement element) ||
                !Enum.IsDefined(typeof(SlimeElement), element) || fields[2] != element.ToString())
                throw new Exception($"Record {rowNumber}: element must be Water, Fire, Grass or Electric.");
            if (!Enum.TryParse(fields[3], false, out SlimeGrade grade) ||
                !Enum.IsDefined(typeof(SlimeGrade), grade) || fields[3] != grade.ToString())
                throw new Exception($"Record {rowNumber}: grade must be Common, Rare, Hero or Legendary.");

            var row = new Row
            {
                Id = id, Name = fields[1], Element = element, Grade = grade,
                Damage = ReadNumber(fields[4], "attackDamage", rowNumber, false),
                Interval = ReadNumber(fields[5], "attackInterval", rowNumber, true),
                Range = ReadNumber(fields[6], "attackRange", rowNumber, false),
                MergeId = fields[7]
            };
            rows.Add(row);
            byId.Add(id, row);
        }

        foreach (Row row in rows)
        {
            if (string.IsNullOrEmpty(row.MergeId)) continue;
            if (row.Grade == SlimeGrade.Legendary)
                throw new Exception($"{row.Id}: Legendary mergeResultId must be empty.");
            if (!byId.TryGetValue(row.MergeId, out Row target))
                throw new Exception($"{row.Id}: merge target '{row.MergeId}' must also appear in this CSV.");
            if (target.Element != row.Element || (int)target.Grade != (int)row.Grade + 1)
                throw new Exception($"{row.Id}: merge target must have the same element and the next grade.");
        }
        return rows;
    }

    private static float ReadNumber(string text, string column, int row, bool positive)
    {
        if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ||
            float.IsNaN(value) || float.IsInfinity(value) || value < 0f || (positive && value <= 0f))
            throw new Exception($"Record {row}: invalid {column}. Use a decimal point; value must be " +
                (positive ? "greater than 0." : "0 or greater."));
        return value;
    }

    private static Dictionary<string, SlimeData> ResolveExistingAssets(List<Row> rows)
    {
        var result = new Dictionary<string, SlimeData>(StringComparer.OrdinalIgnoreCase);
        foreach (Row row in rows) result.Add(row.Id, null);

        foreach (string guid in AssetDatabase.FindAssets("t:SlimeData", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string id = Path.GetFileNameWithoutExtension(path);
            if (!result.ContainsKey(id)) continue;
            if (result[id] != null)
                throw new Exception($"Multiple SlimeData assets match id '{id}'. Keep one matching asset before importing.");
            result[id] = AssetDatabase.LoadAssetAtPath<SlimeData>(path);
        }

        foreach (Row row in rows)
        {
            if (result[row.Id] != null) continue;
            string path = OutputFolder + "/" + row.Id + ".asset";
            if (AssetDatabase.LoadMainAssetAtPath(path) != null || File.Exists(path))
                throw new Exception($"Cannot create {path}: another file already uses this path.");
        }
        return result;
    }

    private static void EnsureFolder(string folder)
    {
        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static Color DefaultColor(SlimeElement element)
    {
        return element switch
        {
            SlimeElement.Water => new Color(0.3f, 0.65f, 1f),
            SlimeElement.Fire => new Color(1f, 0.35f, 0.25f),
            SlimeElement.Grass => new Color(0.3f, 0.85f, 0.35f),
            SlimeElement.Electric => new Color(1f, 0.85f, 0.2f),
            _ => Color.white
        };
    }

    // Supports quoted commas, escaped quotes and newlines inside quoted fields.
    public static List<List<string>> ParseCsv(string csv)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        bool quoted = false;
        bool closedQuote = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];
            if (i == 0 && c == '\uFEFF') continue;

            if (quoted)
            {
                if (c == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"')
                    { field.Append('"'); i++; }
                    else { quoted = false; closedQuote = true; }
                }
                else field.Append(c);
                continue;
            }

            if (c == ',')
            {
                record.Add(field.ToString()); field.Clear(); closedQuote = false;
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n') i++;
                record.Add(field.ToString()); field.Clear(); closedQuote = false;
                if (!IsBlank(record)) records.Add(record);
                record = new List<string>();
            }
            else if (c == '"')
            {
                if (field.Length != 0 || closedQuote)
                    throw new Exception("Invalid CSV quoting.");
                quoted = true;
            }
            else
            {
                if (closedQuote)
                {
                    if (char.IsWhiteSpace(c)) continue;
                    throw new Exception("Invalid character after a quoted CSV field.");
                }
                field.Append(c);
            }
        }

        if (quoted) throw new Exception("CSV contains an unclosed quote.");
        if (field.Length > 0 || record.Count > 0 || closedQuote)
        {
            record.Add(field.ToString());
            if (!IsBlank(record)) records.Add(record);
        }
        return records;
    }

    private static bool IsBlank(List<string> record)
    {
        foreach (string field in record)
            if (!string.IsNullOrWhiteSpace(field)) return false;
        return true;
    }
}
