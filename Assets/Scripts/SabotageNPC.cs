using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageNPC : MonoBehaviour
{
    [Header("NPC")]
    public GameObject npcObject;

    [Header("Animation")]
    public float showTime = 1.5f;

    [Header("Shuffle Animation")]
    public float shuffleDuration = 0.8f;
    public float rotateAngle = 360f;

    private void Awake()
    {
        if (npcObject != null)
            npcObject.SetActive(false);
    }

    public IEnumerator Activate(List<Card> cards)
    {
        if (npcObject != null)
            npcObject.SetActive(true);

        Debug.Log("SABOTAGE NPC ACTIVATED!");

        yield return new WaitForSeconds(showTime);

        // Chỉ tráo vị trí giữa các thẻ CHƯA MATCH.
        // Các thẻ đã lật/matched giữ nguyên vị trí.
        yield return StartCoroutine(ShuffleRemainingCards(cards));

        yield return new WaitForSeconds(0.3f);

        if (npcObject != null)
            npcObject.SetActive(false);
    }

    private IEnumerator ShuffleRemainingCards(List<Card> cards)
    {
        List<Card> remainingCards = new List<Card>();
        List<int> originalSiblingIndices = new List<int>();

        if (cards == null)
            yield break;

        // -------------------------------------------------
        // LẤY CÁC CARD CHƯA MATCH
        // -------------------------------------------------
        foreach (Card card in cards)
        {
            if (card == null)
                continue;

            if (card.IsMatched())
                continue;

            Transform t = card.transform;

            // Card phải là con trực tiếp của CardContainer
            // để sibling index tương ứng với slot trên Grid.
            remainingCards.Add(card);
            originalSiblingIndices.Add(t.GetSiblingIndex());
        }

        if (remainingCards.Count <= 1)
            yield break;

        // -------------------------------------------------
        // SHUFFLE CARD, KHÔNG SHUFFLE CÁC SLOT
        // -------------------------------------------------
        List<Card> shuffledCards =
            new List<Card>(remainingCards);

        for (int i = 0; i < shuffledCards.Count; i++)
        {
            int randomIndex =
                Random.Range(i, shuffledCards.Count);

            Card temp = shuffledCards[i];
            shuffledCards[i] = shuffledCards[randomIndex];
            shuffledCards[randomIndex] = temp;
        }

        // Tránh trường hợp shuffle nhưng kết quả y hệt.
        bool changed = false;

        for (int i = 0; i < remainingCards.Count; i++)
        {
            if (remainingCards[i] != shuffledCards[i])
            {
                changed = true;
                break;
            }
        }

        if (!changed && shuffledCards.Count > 1)
        {
            Card temp = shuffledCards[0];
            shuffledCards[0] = shuffledCards[1];
            shuffledCards[1] = temp;
        }

        Debug.Log(
            "SABOTAGE: Swapping " +
            remainingCards.Count +
            " unopened cards only."
        );

        // -------------------------------------------------
        // ANIMATION:
        // Các card quay tại CHÍNH SLOT CỦA CHÚNG,
        // sau đó đổi sibling index.
        // -------------------------------------------------
        List<Coroutine> rotations = new List<Coroutine>();

        foreach (Card card in remainingCards)
        {
            if (card != null)
            {
                rotations.Add(
                    StartCoroutine(
                        RotateCard(card.transform)
                    )
                );
            }
        }

        // Chờ animation quay.
        yield return new WaitForSeconds(
            shuffleDuration
        );

        // -------------------------------------------------
        // ĐỔI CARD VÀO CÁC SLOT CŨ CỦA CARD CHƯA MATCH
        //
        // Ví dụ:
        // slot:       [0] [1] [2] [3] [4]
        // matched:     X       X
        // before:      A   B   X   C   X
        //
        // sau shuffle:
        //             C   A   X   B   X
        //
        // X không di chuyển.
        // A/B/C chỉ đổi chỗ cho nhau.
        // -------------------------------------------------

        // Vì SetSiblingIndex làm thay đổi index ngay lập tức,
        // xử lý theo mapping ổn định bằng cách đặt từ cuối xuống.
        List<CardSlot> assignments =
            new List<CardSlot>();

        for (int i = 0; i < originalSiblingIndices.Count; i++)
        {
            assignments.Add(
                new CardSlot(
                    shuffledCards[i],
                    originalSiblingIndices[i]
                )
            );
        }

        // Sắp xếp slot tăng dần rồi đặt từng card.
        assignments.Sort(
            (a, b) => a.siblingIndex.CompareTo(b.siblingIndex)
        );

        foreach (CardSlot assignment in assignments)
        {
            if (assignment.card == null)
                continue;

            assignment.card.transform.SetSiblingIndex(
                assignment.siblingIndex
            );
        }

        // Đợi một frame để GridLayoutGroup cập nhật.
        yield return null;

        Debug.Log(
            "SABOTAGE: Unopened cards swapped among " +
            "their existing slots. Matched cards stayed in place."
        );
    }

    private IEnumerator RotateCard(
        Transform cardTransform)
    {
        if (cardTransform == null)
            yield break;

        Quaternion originalRotation =
            cardTransform.localRotation;

        float elapsed = 0f;

        while (elapsed < shuffleDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / shuffleDuration
                );

            // Quay nhiều vòng để người chơi dễ nhận biết.
            float angle =
                Mathf.Lerp(
                    0f,
                    rotateAngle,
                    t
                );

            cardTransform.localRotation =
                originalRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );

            yield return null;
        }

        cardTransform.localRotation =
            originalRotation;
    }

    private class CardSlot
    {
        public Card card;
        public int siblingIndex;

        public CardSlot(Card card, int siblingIndex)
        {
            this.card = card;
            this.siblingIndex = siblingIndex;
        }
    }
}