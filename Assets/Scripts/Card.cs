using System.Collections;
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
    private bool isBoom = false;

    private void Awake()
    {
        button = back.GetComponentInChildren<Button>();
    }

    public void Setup(int id, Sprite sprite)
    {
        Setup(id, sprite, false);
    }

    public void Setup(int id, Sprite sprite, bool boom)
    {
        cardId = id;
        isBoom = boom;
        frontImage.sprite = sprite;

        isFlipped = false;
        isMatched = false;

        ShowBack();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        if (isFlipped || isMatched)
            return;

        // Nếu đang ở Normal Mode
        if (NormalGameManager.Instance != null)
        {
            NormalGameManager.Instance.CardClicked(this);
            return;
        }

        // Nếu đang ở AI Mode
        if (AIGameManager.Instance != null)
        {
            AIGameManager.Instance.CardClicked(this);
        }
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

    public bool IsBoom()
    {
        return isBoom;
    }

    // =========================================
    // NPC HELPER - CARD SHAKE
    // =========================================

    public IEnumerator Shake(float duration = 0.45f, float strength = 12f)
    {
        RectTransform rect = GetComponent<RectTransform>();

        if (rect == null)
            yield break;

        Vector2 originalPosition = rect.anchoredPosition;

        float timer = 0f;

        while (timer < duration)
        {
            float x = Random.Range(-strength, strength);
            float y = Random.Range(-strength, strength);

            rect.anchoredPosition = originalPosition + new Vector2(x, y);

            timer += Time.deltaTime;

            yield return null;
        }

        rect.anchoredPosition = originalPosition;
    }

    // =========================================
    // SHUFFLE ANIMATION - DI CHUYỂN VÀ XOAY CARD
    // =========================================

    public IEnumerator MoveToPosition(Vector3 targetPos, float duration = 0.35f)
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) yield break;

        Vector3 startPos = rect.anchoredPosition;
        Quaternion startRot = rect.localRotation;

        // Tạo góc xoay nhẹ (ngẫu nhiên từ -15 đến 15 độ) để tạo hiệu ứng xáo trộn tự nhiên
        Quaternion targetRot = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;

            // Dùng SmoothStep để chuyển động êm hơn
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            rect.anchoredPosition = Vector3.Lerp(startPos, targetPos, smoothProgress);
            rect.localRotation = Quaternion.Slerp(startRot, targetRot, smoothProgress);

            yield return null;
        }

        rect.anchoredPosition = targetPos;

        // Trả lại góc xoay thẳng chuẩn (0,0,0)
        float resetTimer = 0f;
        float resetDuration = 0.15f;
        while (resetTimer < resetDuration)
        {
            resetTimer += Time.deltaTime;
            rect.localRotation = Quaternion.Slerp(targetRot, Quaternion.identity, resetTimer / resetDuration);
            yield return null;
        }

        rect.localRotation = Quaternion.identity;
    }
}