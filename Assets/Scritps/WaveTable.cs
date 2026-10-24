using System;
using System.Collections.Generic;
using UnityEngine;




[Serializable]
public class WaveRow
{
    public int wave;
    public string appearanceId;

    [Min(1f)]
    public float health = 30f;

    public float goldMultiplier = 1f;
}

[Serializable]
public class WaveAppearance
{
    public string id;
    public Sprite sprite;
    public Color color = Color.white;

    public EnemyWalkData walkAnimation;
}

[CreateAssetMenu(
    fileName = "WaveTable",
    menuName = "Slime Defense/Wave Table"
)]
public class WaveTable : ScriptableObject
{
    public List<WaveRow> waves = new();
    public List<WaveAppearance> appearances = new();

    public WaveRow GetWave(int number)
    {
        return waves.Find(row => row.wave == number);
    }

    public WaveAppearance GetAppearance(string id)
    {
        return appearances.Find(item => item.id == id);
    }
}
