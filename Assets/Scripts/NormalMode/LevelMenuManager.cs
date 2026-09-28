using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenuManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public LevelButton[] levelButtons;

    private void Start()
    {
        UpdateLevelButtons();
    }

    // =========================
    // UPDATE LEVEL BUTTONS
    // =========================

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i + 1;

            bool completed =
                PlayerPrefs.GetInt("LevelCompleted_" + level, 0) == 1;

            // Level 1 luôn mở
            if (level == 1)
            {
                if (completed)
                {
                    levelButtons[i].SetCompleted();
                }
                else
                {
                    levelButtons[i].SetNormal();
                }

                continue;
            }

            // Kiểm tra level phía trước đã hoàn thành chưa
            bool previousCompleted =
                PlayerPrefs.GetInt(
                    "LevelCompleted_" + (level - 1),
                    0
                ) == 1;

            if (!previousCompleted)
            {
                levelButtons[i].SetLocked();
            }
            else if (completed)
            {
                levelButtons[i].SetCompleted();
            }
            else
            {
                levelButtons[i].SetNormal();
            }
        }
    }

    // =========================
    // SELECT LEVEL
    // =========================

    public void SelectLevel(int level)
    {
        bool completed =
            PlayerPrefs.GetInt("LevelCompleted_" + level, 0) == 1;

        // Level 1 luôn được phép chơi
        if (level != 1)
        {
            bool previousCompleted =
                PlayerPrefs.GetInt(
                    "LevelCompleted_" + (level - 1),
                    0
                ) == 1;

            if (!previousCompleted)
            {
                return;
            }
        }

        PlayerPrefs.SetInt("SelectedLevel", level);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GamePlay");
    }

    // =========================
    // HOME
    // =========================

    public void GoHome()
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene("SettingsScene",
            LoadSceneMode.Additive
        );
    }
}