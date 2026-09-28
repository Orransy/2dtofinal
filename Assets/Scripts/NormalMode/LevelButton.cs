using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [Header("Level")]
    public int levelNumber;

    [Header("Button")]
    public Button button;

    [Header("UI")]
    public TMP_Text levelText;
    public GameObject lockIcon;
    public GameObject completedIcon;

    public void SetLocked()
    {
        button.interactable = false;

        levelText.gameObject.SetActive(true);

        lockIcon.SetActive(true);
        completedIcon.SetActive(false);
    }

    public void SetNormal()
    {
        button.interactable = true;

        levelText.gameObject.SetActive(true);

        lockIcon.SetActive(false);
        completedIcon.SetActive(false);
    }

    public void SetCompleted()
    {
        button.interactable = true;

        levelText.gameObject.SetActive(true);

        lockIcon.SetActive(false);
        completedIcon.SetActive(true);
    }
}