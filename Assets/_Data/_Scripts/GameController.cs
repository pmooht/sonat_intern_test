using UnityEngine;

// Orchestrator: khởi động game, kết nối các component, lắng nghe event
// Đây là nơi để hook thêm UI, Sound, Analytics, v.v. về sau
public class GameController : MonoBehaviour
{
  [SerializeField] private LevelManager levelManager;
  [SerializeField] private InputHandler inputHandler;

  private void Start()
  {
    // Subscribe event từ LevelManager
    levelManager.OnLevelComplete += HandleLevelComplete;

    // Bắt đầu game với level đầu tiên
    levelManager.SpawnLevel();
  }

  private void OnDestroy()
  {
    // Hủy subscribe để tránh memory leak
    if (levelManager != null)
      levelManager.OnLevelComplete -= HandleLevelComplete;
  }

  // Được gọi khi LevelManager thông báo level hoàn thành
  // → Thêm logic UI / Sound / Analytics tại đây
  private void HandleLevelComplete()
  {
    Debug.Log("[GameController] Level hoàn thành!");
    // TODO: ShowLevelCompleteUI();
    // TODO: PlayCompleteSFX();
  }
}
