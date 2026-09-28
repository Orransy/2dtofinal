using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperNPC : MonoBehaviour
{
    [Header("NPC")]
    public GameObject npcObject;

    [Header("Animation")]
    public float showTime = 0.5f;
    public float shakeDuration = 1.5f;
    public float shakeAngle = 8f;
    public float shakeSpeed = 20f;

    private void Awake()
    {
        if (npcObject != null)
            npcObject.SetActive(false);
    }

    // Được AIGameManager gọi SAU KHI AI hoàn thành lượt.
    // Helper chỉ rung 2 card, không tự lật card và không đổi lượt.
    public IEnumerator GiveHint(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
        {
            Debug.LogWarning("HELPER NPC: Card list is empty.");
            yield break;
        }

        Card hintCard1 = null;
        Card hintCard2 = null;

        // Tìm một cặp chưa match và đang úp.
        for (int i = 0; i < cards.Count; i++)
        {
            Card card1 = cards[i];

            if (card1 == null || card1.IsMatched() || card1.IsFlipped())
                continue;

            for (int j = i + 1; j < cards.Count; j++)
            {
                Card card2 = cards[j];

                if (card2 == null || card2.IsMatched() || card2.IsFlipped())
                    continue;

                if (card1.cardId == card2.cardId)
                {
                    hintCard1 = card1;
                    hintCard2 = card2;
                    break;
                }
            }

            if (hintCard1 != null && hintCard2 != null)
                break;
        }

        if (hintCard1 == null || hintCard2 == null)
        {
            Debug.Log("HELPER NPC: No available pair found.");
            yield break;
        }

        Debug.Log("HELPER NPC: Hint pair = " + hintCard1.cardId);

        if (npcObject != null)
            npcObject.SetActive(true);

        yield return new WaitForSeconds(showTime);

        yield return StartCoroutine(
            ShakeTwoCards(hintCard1, hintCard2)
        );

        if (npcObject != null)
            npcObject.SetActive(false);
    }

    private IEnumerator ShakeTwoCards(Card card1, Card card2)
    {
        if (card1 == null || card2 == null)
            yield break;

        Transform t1 = card1.transform;
        Transform t2 = card2.transform;

        Quaternion originalRotation1 = t1.localRotation;
        Quaternion originalRotation2 = t2.localRotation;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            float angle =
                Mathf.Sin(elapsed * shakeSpeed) * shakeAngle;

            // Rung ngược chiều nhau để dễ nhận biết.
            t1.localRotation =
                originalRotation1 * Quaternion.Euler(0f, 0f, angle);

            t2.localRotation =
                originalRotation2 * Quaternion.Euler(0f, 0f, -angle);

            yield return null;
        }

        t1.localRotation = originalRotation1;
        t2.localRotation = originalRotation2;
    }
}