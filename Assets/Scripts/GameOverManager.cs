using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject resultPanel;

    public TMP_Text resultTitle;

    [Header("Effect Groups")]
    public GameObject gameOverEffects;
    public GameObject winEffects;

    [Header("Effect Animators")]
    public Animator[] gameOverAnimators;
    public Animator[] winAnimators;

    private void Start()
    {
        // Khi bắt đầu scene, ẩn cả 2 loại effect
        gameOverEffects.SetActive(false);
        winEffects.SetActive(false);
    }

    public void ShowResult(int playerScore, int aiScore)
    {
        resultPanel.SetActive(true);

        if (playerScore > aiScore)
        {
            resultTitle.text = "YOU WIN!";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWinSound();
            }

            ShowWinEffects();
        }
        else if (aiScore > playerScore)
        {
            resultTitle.text = "YOU LOSE!";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOverSound();
            }

            ShowGameOverEffects();
        }
        else
        {
            resultTitle.text = "DRAW!";
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScene");
    }

    public void GoToLevelMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("NormalGameScene");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene(
            "SettingsScene",
            LoadSceneMode.Additive
        );
    }

    public void ShowWin()
    {
        resultPanel.SetActive(true);

        resultTitle.text = "YOU WIN!";

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinSound();
        }

        ShowWinEffects();
    }

    public void ShowGameOver()
    {
        resultPanel.SetActive(true);

        resultTitle.text = "GAME OVER";

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }

        ShowGameOverEffects();
    }

    private void ShowWinEffects()
    {
        // Tắt Game Over
        gameOverEffects.SetActive(false);

        // Bật Win
        winEffects.SetActive(true);

        // Chạy toàn bộ Win Animator
        if (winAnimators != null)
        {
            foreach (Animator animator in winAnimators)
            {
                if (animator != null)
                {
                    animator.Rebind();
                    animator.Update(0f);
                    animator.Play("WinEffect", 0, 0f);
                }
            }
        }
    }

    private void ShowGameOverEffects()
    {
        // Tắt Win
        winEffects.SetActive(false);

        // Bật Game Over
        gameOverEffects.SetActive(true);

        // Chạy toàn bộ Game Over Animator
        if (gameOverAnimators != null)
        {
            foreach (Animator animator in gameOverAnimators)
            {
                if (animator != null)
                {
                    animator.Rebind();
                    animator.Update(0f);
                    animator.Play("GameOverEffect", 0, 0f);
                }
            }
        }
    }
}