using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// UIManager cho scene SelectLevel:
// Tự động tạo/gán nút level từ LevelListSO,
// lưu lựa chọn vào PlayerPrefs rồi load scene Gameplay
public class UIManager : MonoBehaviour
{
  [SerializeField] private LevelListSO levelList;
  [SerializeField] private Transform   levelButtonContainer; // LevelPanel
  [SerializeField] private string      gameplaySceneName = "Gameplay";
  // Tên GameObject lock image là con của mỗi Button
  [SerializeField] private string      lockImageName     = "Lock";

  private void Start()
  {
    SetupLevelButtons();
  }

  // Tự động gán OnClick cho từng Button trong LevelPanel theo thứ tự index
  private void SetupLevelButtons()
  {
    if (levelList == null)
    {
      Debug.LogError("[UIManager] Chưa gán levelList!");
      return;
    }

    Button[] buttons = levelButtonContainer.GetComponentsInChildren<Button>();

    for (int i = 0; i < buttons.Length; i++)
    {
      // Tìm lock image trong các con của button
      Transform lockImg = buttons[i].transform.Find(lockImageName);

      if (i >= levelList.levels.Length)
      {
        // Level chưa có data: hiện lock, tắt tương tác
        buttons[i].interactable = false;
        if (lockImg != null) lockImg.gameObject.SetActive(true);
        continue;
      }

      // Level hợp lệ: ẩn lock, bật tương tác
      buttons[i].interactable = true;
      if (lockImg != null) lockImg.gameObject.SetActive(false);

      int levelIndex = i; // capture cho closure
      buttons[i].onClick.RemoveAllListeners();
      buttons[i].onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
    }
  }

  // Lưu index được chọn và chuyển sang scene Gameplay
  private void OnLevelButtonClicked(int levelIndex)
  {
    Debug.Log($"[UIManager] Chọn Level {levelIndex + 1}");
    PlayerPrefs.SetInt("SelectedLevel", levelIndex);
    PlayerPrefs.Save();
    SceneManager.LoadScene(gameplaySceneName);
  }
}
