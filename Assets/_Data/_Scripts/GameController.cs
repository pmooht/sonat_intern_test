using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
  [SerializeField] private LevelManager levelManager;
  [SerializeField] private InputHandler inputHandler;
  [SerializeField] private PowerUpManager powerUpManager;

  [Header("Win UI")]
  [SerializeField] private GameObject winPanel;
  [SerializeField] private Button btnContinue;
  [SerializeField] private Button btnExitWin;

  [Header("Lose UI")]
  [SerializeField] private GameObject losePanel;
  [SerializeField] private Button btnRestartLose;
  [SerializeField] private Button btnExitLose;

  [Header("Gameplay UI")]
  [SerializeField] private Button btnRestart;
  [SerializeField] private Button btnExit;

  [SerializeField] private string selectLevelSceneName = "SelectLevel";

  private void Start()
  {
    SoundManager.Instance?.PlayGameplayBGM();

    winPanel?.SetActive(false);
    losePanel?.SetActive(false);

    levelManager.OnLevelComplete += HandleLevelComplete;

    btnContinue?.onClick.AddListener(OnContinueClicked);
    btnRestartLose?.onClick.AddListener(OnRestartClicked);
    btnExitWin?.onClick.AddListener(OnExitClicked);
    btnExitLose?.onClick.AddListener(OnExitClicked);
    btnRestart?.onClick.AddListener(OnRestartClicked);
    btnExit?.onClick.AddListener(OnExitClicked);

    levelManager.SpawnLevel();
  }

  private void OnDestroy()
  {
    if (levelManager != null)
      levelManager.OnLevelComplete -= HandleLevelComplete;
  }

  private void HandleLevelComplete()
  {
    bool hasNext = levelManager.HasNextLevel;

    winPanel?.SetActive(true);

    if (btnContinue != null)
      btnContinue.gameObject.SetActive(hasNext);
  }

  private void OnContinueClicked()
  {
    winPanel?.SetActive(false);
    powerUpManager?.ResetAllUses();
    levelManager.LoadNextLevel();
  }

  private void OnRestartClicked()
  {
    winPanel?.SetActive(false);
    losePanel?.SetActive(false);
    powerUpManager?.ResetAllUses();
    levelManager.RestartLevel();
  }

  private void OnExitClicked()
  {
    SceneManager.LoadScene(selectLevelSceneName);
  }

  public void ShowLosePanel()
  {
    losePanel?.SetActive(true);
  }
}
