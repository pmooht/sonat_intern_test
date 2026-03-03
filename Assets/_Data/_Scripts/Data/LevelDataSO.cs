using UnityEngine;

[CreateAssetMenu(fileName = "Level Data", menuName = "CustomLevel/Level Data")]
public class LevelDataSO : ScriptableObject {
    public int levelIndex;
    public BottleData[] bottles;  // mỗi phần tử = 1 chai
}