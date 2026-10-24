using UnityEngine;




[CreateAssetMenu(
    fileName = "EnemyWalkData",
    menuName = "Slime Defense/Enemy Walk Data"
)]
public class EnemyWalkData : ScriptableObject
{
    [Min(1f)] public float framesPerSecond = 6f;

    public Sprite[] down;
    public Sprite[] up;
    public Sprite[] left;
    public Sprite[] right;
}
