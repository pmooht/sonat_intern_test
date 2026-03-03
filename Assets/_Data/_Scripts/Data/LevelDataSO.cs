using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "CustomLevel/Level Data")]
public class LevelDataSO : ScriptableObject
{
  public int levelIndex;
  public Sprite background; // Background riêng của mỗi level
  public BottleData[] bottles;
}