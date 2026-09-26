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
}