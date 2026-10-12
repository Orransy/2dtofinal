using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Story UI")]
    public TMP_Text storyText;
    public Button nextButton;

    [Header("Story Images")]
    public Image backgroundImage;
    public Image illustrationImage;

    public Sprite[] backgroundPages;
    public Sprite[] illustrationPages;

    [Header("Character")]
    public UnityEngine.UI.Image characterImage;
    public Sprite[] characterSprites;
    public Vector2[] characterAnchors;

    [Header("Story Content")]
    [TextArea(3, 6)]
    public string[] storyPages;

    private int currentPage = 0;

    private void Start()
    {
        ShowPage();
    }

    private void ShowPage()
    {
        if (storyPages == null || storyPages.Length == 0)
        {
            Debug.LogWarning("Chưa có nội dung cốt truyện!");
            return;
        }

        storyText.text = storyPages[currentPage];

        if (backgroundImage != null &&
            backgroundPages != null &&
            currentPage < backgroundPages.Length)
        {
            backgroundImage.sprite = backgroundPages[currentPage];
        }

        if (illustrationImage != null &&
            illustrationPages != null &&
            currentPage < illustrationPages.Length)
        {
            illustrationImage.sprite = illustrationPages[currentPage];
        }

        // Đổi ảnh nhân vật theo trang
        if (characterImage != null &&
            characterSprites != null &&
            currentPage < characterSprites.Length)
        {
            characterImage.sprite = characterSprites[currentPage];
        }

        // Đổi vị trí nhân vật theo trang
        if (characterImage != null &&
            characterAnchors != null &&
            currentPage < characterAnchors.Length)
        {
            RectTransform rect = characterImage.rectTransform;
            Vector2 pos = characterAnchors[currentPage];

            rect.anchorMin = pos;
            rect.anchorMax = pos;
            rect.anchoredPosition = Vector2.zero;
        }

        TMP_Text buttonText =
            nextButton.GetComponentInChildren<TMP_Text>();

        buttonText.text =
            currentPage == storyPages.Length - 1 ? "START" : "NEXT";
    }

    public void NextPage()
    {
        if (currentPage < storyPages.Length - 1)
        {
            currentPage++;
            ShowPage();
        }
        else
        {
            GoToHome();
        }
    }

    public void SkipStory()
    {
        GoToHome();
    }

    private void GoToHome()
    {
        SceneManager.LoadScene("HomeScene");
    }
}