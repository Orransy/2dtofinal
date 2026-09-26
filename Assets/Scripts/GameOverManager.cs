using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject resultPanel;

    public TMP_Text resultTitle;

    public void ShowResult(int playerScore, int aiScore)
    {
        resultPanel.SetActive(true);

        if (playerScore > aiScore)
        {
            resultTitle.text = "YOU WIN!";
        }
        else if (aiScore > playerScore)
        {
            resultTitle.text = "ROBOT WIN!";
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
}