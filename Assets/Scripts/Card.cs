using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card UI")]
    public GameObject back;
    public GameObject front;
    public Image frontImage;

    [Header("Card Data")]
    public int cardId;

    private Button button;

    private bool isFlipped = false;
    private bool isMatched = false;

    private void Awake()
    {
        button = back.GetComponentInChildren<Button>();
    }

    public void Setup(int id, Sprite sprite)
    {
        cardId = id;

        // Gán hình cho mặt trước
        frontImage.sprite = sprite;

        // Ban đầu úp thẻ
        ShowBack();

        // Xóa listener cũ
        button.onClick.RemoveAllListeners();

        // Thêm sự kiện click
        button.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        if (isFlipped || isMatched)
            return;

        AIGameManager.Instance.CardClicked(this);
    }

    public void ShowBack()
    {
        back.SetActive(true);
        front.SetActive(false);

        isFlipped = false;
    }

    public void ShowFront()
    {
        back.SetActive(false);
        front.SetActive(true);

        isFlipped = true;
    }

    public void SetMatched()
    {
        isMatched = true;
        isFlipped = true;

        back.SetActive(false);
        front.SetActive(true);

        button.interactable = false;
    }

    public bool IsFlipped()
    {
        return isFlipped;
    }

    public bool IsMatched()
    {
        return isMatched;
    }
}