using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
  [SerializeField] private LevelListSO levelList;
  [SerializeField] private GameObject bottlePrefab;
  [SerializeField] private float bottleSpacing = 1.5f;
  [SerializeField] private SpriteRenderer backgroundRenderer;
  [SerializeField] private TMP_Text levelText;

  private int currentLevelIndex = 0;
  public event Action OnLevelComplete;

  private List<BottleController> spawnedBottles = new List<BottleController>();

  private void Awake()
  {
    currentLevelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
  }

  public void SpawnLevel()
  {
    // Destroy các bình cũ trước khi spawn level mới
    foreach (BottleController b in spawnedBottles)
      if (b != null) Destroy(b.gameObject);
    spawnedBottles.Clear();

    if (levelList == null || levelList.levels.Length == 0 || bottlePrefab == null)
      return;

    if (currentLevelIndex >= levelList.levels.Length)
      return;

    LevelDataSO levelData = levelList.levels[currentLevelIndex];

    if (levelData.levelPrefab == null)
      return;

    LevelConfig config = levelData.levelPrefab.GetComponent<LevelConfig>();
    if (config == null)
      return;

    if (levelText != null)
      levelText.text = $"Level {currentLevelIndex + 1}";

    if (backgroundRenderer != null && config.backgroundRenderer != null)
      backgroundRenderer.sprite = config.backgroundRenderer.sprite;

    if (config.palette == null || config.palette.Length == 0)
      return;

    const int layersPerBottle = 4;
    int totalSlots = config.filledBottleCount * layersPerBottle;

    if (totalSlots % config.palette.Length != 0)
      return;

    int slotsPerColor = totalSlots / config.palette.Length;

    List<Color> colorPool = new List<Color>();
    foreach (Color c in config.palette)
      for (int i = 0; i < slotsPerColor; i++)
        colorPool.Add(c);

    for (int i = colorPool.Count - 1; i > 0; i--)
    {
      int j        = UnityEngine.Random.Range(0, i + 1);
      Color tmp    = colorPool[i];
      colorPool[i] = colorPool[j];
      colorPool[j] = tmp;
    }

    int colorIdx = 0;
    List<BottleData> filledBottles = new List<BottleData>();

    for (int i = 0; i < config.filledBottleCount; i++)
    {
      BottleData bd = new BottleData
      {
        numberOfColors = layersPerBottle,
        colors         = new Color[layersPerBottle]
      };
      for (int c = 0; c < layersPerBottle; c++)
        bd.colors[c] = colorPool[colorIdx++];
      filledBottles.Add(bd);
    }

    for (int i = filledBottles.Count - 1; i > 0; i--)
    {
      int j = UnityEngine.Random.Range(0, i + 1);
      BottleData tmp = filledBottles[i];
      filledBottles[i] = filledBottles[j];
      filledBottles[j] = tmp;
    }
    
    List<BottleData> allBottles = new List<BottleData>(filledBottles);
    for (int i = 0; i < config.emptyBottleCount; i++)
      allBottles.Add(new BottleData { numberOfColors = 0, colors = new Color[layersPerBottle] });

    int count = allBottles.Count;
    float totalWidth = (count - 1) * bottleSpacing;
    float startX = -totalWidth / 2f;

    for (int i = 0; i < count; i++)
    {
      Vector3 spawnPos = new Vector3(startX + i * bottleSpacing, 0f, 0f);
      GameObject go = Instantiate(bottlePrefab, spawnPos, Quaternion.identity);
      BottleController bottle = go.GetComponent<BottleController>();

      if (bottle == null) continue;

      bottle.InitFromData(allBottles[i]);
      spawnedBottles.Add(bottle);
    }
  }

  public void CheckLevelComplete()
  {
    foreach (BottleController bottle in spawnedBottles)
    {
      if (bottle != null && !bottle.IsSolved()) return;
    }

    foreach (BottleController bottle in spawnedBottles)
      if (bottle != null) Destroy(bottle.gameObject);
    spawnedBottles.Clear();

    OnLevelComplete?.Invoke();
  }

  public void AddExtraBottle()
  {
    BottleData emptyData = new BottleData
    {
      numberOfColors = 0,
      colors         = new Color[4]
    };

    GameObject go = Instantiate(bottlePrefab, Vector3.zero, Quaternion.identity);
    BottleController bottle = go.GetComponent<BottleController>();
    if (bottle == null) { Destroy(go); return; }

    bottle.InitFromData(emptyData);
    spawnedBottles.Add(bottle);

    StartCoroutine(RepositionAllBottles());
  }

  /// <summary>
  /// Tính lại vị trí nằm ngang cho tất cả bình dựa trên số lượng hiện tại,
  /// rồi tween mượt về vị trí mới.
  /// </summary>
  private IEnumerator RepositionAllBottles()
  {
    int   count      = spawnedBottles.Count;
    float totalWidth = (count - 1) * bottleSpacing;
    float startX     = -totalWidth / 2f;

    Vector3[] fromPositions = new Vector3[count];
    Vector3[] toPositions   = new Vector3[count];
    for (int i = 0; i < count; i++)
    {
      if (spawnedBottles[i] == null) continue;
      fromPositions[i] = spawnedBottles[i].transform.position;
      toPositions[i]   = new Vector3(startX + i * bottleSpacing, 0f, 0f);
    }

    float t = 0f;
    const float duration = 0.4f;
    while (t < duration)
    {
      t += Time.deltaTime;
      float lerp = Mathf.SmoothStep(0f, 1f, t / duration);
      for (int i = 0; i < count; i++)
      {
        if (spawnedBottles[i] == null) continue;
        spawnedBottles[i].transform.position = Vector3.Lerp(fromPositions[i], toPositions[i], lerp);
      }
      yield return null;
    }

    for (int i = 0; i < count; i++)
    {
      if (spawnedBottles[i] == null) continue;
      spawnedBottles[i].transform.position = toPositions[i];
      spawnedBottles[i].SetOriginalPosition(toPositions[i]);
    }
  }

  public IReadOnlyList<BottleController> GetAllBottles() => spawnedBottles;

  public bool HasNextLevel => currentLevelIndex + 1 < levelList.levels.Length;

  public void LoadNextLevel()
  {
    currentLevelIndex++;
    SpawnLevel();
  }

  public void RestartLevel()
  {
    SpawnLevel();
  }
}
