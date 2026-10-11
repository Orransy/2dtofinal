using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeManager : MonoBehaviour
{
    public void PlayNormalMode()
    {
        SceneManager.LoadScene("NormalGameScene");
    }

    public void PlayAIMode()
    {
        SceneManager.LoadScene("AIGameScene");
    }

    public void GoToSettings()
    {
        SceneManager.LoadScene("SettingsScene",
            LoadSceneMode.Additive
        );
    }

    public void Storyline()
    {
        SceneManager.LoadScene("StorylineScene");
    }
}