using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashManager : MonoBehaviour
{
    public Slider loadingBar;

    private void Start()
    {
        StartCoroutine(LoadHome());
    }

    IEnumerator LoadHome()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.5f;

            loadingBar.value = progress;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("HomeScene");
    }
}