using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
  [SerializeField] private LevelListSO levelList;
  [SerializeField] private Transform levelButtonContainer;
  [SerializeField] private string gameplaySceneName = "Gameplay";
  [SerializeField] private string lockImageName = "Lock";

  private void Start()
    {
      SoundManager.Instance?.PlayMenuBGM();
      SetupLevelButtons();
    }

  private void SetupLevelButtons()
    {
      if (levelList == null)
        {
          return;
        }

      Button[] buttons = levelButtonContainer.GetComponentsInChildren<Button>();

      for (int i = 0; i < buttons.Length; i++)
        {
          Transform lockImg = buttons[i].transform.Find(lockImageName);

          if (i >= levelList.levels.Length)
            {
              buttons[i].interactable = false;
              if (lockImg != null) lockImg.gameObject.SetActive(true);
              continue;
            }

          buttons[i].interactable = true;
          if (lockImg != null) lockImg.gameObject.SetActive(false);

          int levelIndex = i;
          buttons[i].onClick.RemoveAllListeners();
          buttons[i].onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
        }
    }

  private void OnLevelButtonClicked(int levelIndex)
    {
      PlayerPrefs.SetInt("SelectedLevel", levelIndex);
      PlayerPrefs.Save();
      SceneManager.LoadScene(gameplaySceneName);
    }
}
