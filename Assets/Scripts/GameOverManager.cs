using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject resultPanel;

    public TMP_Text resultTitle;

    public ParticleSystem[] winFireworks;

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
        }
        else if (aiScore > playerScore)
        {
            resultTitle.text = "ROBOT WIN!";

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOverSound();
            }
        }
        else
        {
            resultTitle.text = "DRAW!";
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        SceneManager.LoadScene("SettingsScene",
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

        Debug.Log("SHOW WIN - FIREWORKS");

        if (winFireworks != null)
        {
            foreach (ParticleSystem fireworks in winFireworks)
            {
                if (fireworks != null)
                {
                    fireworks.Play();
                }
            }
        }
    }

    public void ShowGameOver()
    {
        resultPanel.SetActive(true);

        resultTitle.text = "GAME OVER";

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }
    }
}