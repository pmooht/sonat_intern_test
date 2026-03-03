using UnityEngine;

[CreateAssetMenu(fileName = "Level List", menuName = "CustomLevel/Level List")]
public class LevelListSO : ScriptableObject
{
  public LevelDataSO[] levels;
}
