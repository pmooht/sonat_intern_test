using System;
using System.Collections.Generic;
using UnityEngine;

// Chịu trách nhiệm: spawn chai, xáo trộn màu, quản lý tiến trình level
public class LevelManager : MonoBehaviour
{
  [SerializeField] private LevelListSO levelList;
  [SerializeField] private GameObject bottlePrefab;
  [SerializeField] private float bottleSpacing = 1.5f;
  [SerializeField] private SpriteRenderer backgroundRenderer; // SpriteRenderer nền game

  private int currentLevelIndex = 0;

  // Event thông báo khi 1 level hoàn thành
  // → GameController, UI, Sound Manager có thể subscribe
  public event Action OnLevelComplete;

  private void Awake()
  {
    // Đọc level được chọn từ scene SelectLevel (mặc định 0)
    currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
  }

  public void SpawnLevel()
  {
    if (levelList == null || levelList.levels.Length == 0 || bottlePrefab == null)
    {
      Debug.LogError("[LevelManager] Thiếu levelList hoặc bottlePrefab!");
      return;
    }

    LevelDataSO levelData = levelList.levels[currentLevelIndex];

    // Áp dụng background của level
    if (backgroundRenderer != null && levelData.background != null)
      backgroundRenderer.sprite = levelData.background;

    BottleData[] bottles = levelData.bottles;

    // ── Gom toàn bộ màu vào pool ──────────────────────────────────
    List<Color> colorPool = new List<Color>();
    foreach (BottleData b in bottles)
      for (int i = 0; i < b.numberOfColors; i++)
        colorPool.Add(b.colors[i]);

    // ── Fisher-Yates shuffle ───────────────────────────────────────
    for (int i = colorPool.Count - 1; i > 0; i--)
    {
      int j = UnityEngine.Random.Range(0, i + 1);
      Color tmp = colorPool[i];
      colorPool[i] = colorPool[j];
      colorPool[j] = tmp;
    }

    // ── Phân phối lại màu vào từng chai ───────────────────────────
    int colorIdx = 0;
    BottleData[] shuffledBottles = new BottleData[bottles.Length];
    for (int i = 0; i < bottles.Length; i++)
    {
      shuffledBottles[i] = new BottleData
      {
        numberOfColors = bottles[i].numberOfColors,
        colors         = new Color[4]
      };
      for (int c = 0; c < bottles[i].numberOfColors; c++)
        shuffledBottles[i].colors[c] = colorPool[colorIdx++];
    }

    // ── Spawn chai ─────────────────────────────────────────────────
    int count = shuffledBottles.Length;
    float totalWidth = (count - 1) * bottleSpacing;
    float startX = -totalWidth / 2f;

    for (int i = 0; i < count; i++)
    {
      Vector3 spawnPos = new Vector3(startX + i * bottleSpacing, 0f, 0f);
      GameObject go = Instantiate(bottlePrefab, spawnPos, Quaternion.identity);
      BottleController bottle = go.GetComponent<BottleController>();

      if (bottle == null)
      {
        Debug.LogError("[LevelManager] bottlePrefab không có BottleController!");
        continue;
      }

      bottle.InitFromData(shuffledBottles[i]);
    }
  }

  // Được gọi bởi InputHandler mỗi khi 1 lần đổ màu hoàn tất
  public void CheckLevelComplete()
  {
    BottleController[] allBottles = FindObjectsByType<BottleController>(FindObjectsSortMode.None);
    foreach (BottleController bottle in allBottles)
    {
      if (!bottle.IsSolved()) return;
    }

    // Tất cả đã solved → dọn dẹp chai cũ
    foreach (BottleController bottle in allBottles)
      Destroy(bottle.gameObject);

    // Thông báo ra ngoài (UI, Sound, v.v.)
    OnLevelComplete?.Invoke();

    // Tiến sang level tiếp theo
    currentLevelIndex++;
    if (currentLevelIndex < levelList.levels.Length)
    {
      Debug.Log($"[LevelManager] Bắt đầu Level {currentLevelIndex + 1}");
      SpawnLevel();
    }
    else
    {
      Debug.Log("[LevelManager] Chúc mừng! Bạn đã hoàn thành tất cả các level!");
    }
  }
}
