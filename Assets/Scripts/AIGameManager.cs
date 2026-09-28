using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AIGameManager : MonoBehaviour
{
    public static AIGameManager Instance;

    [Header("NPC")]
    public HelperNPC helperNPC;
    public SabotageNPC sabotageNPC;

    [Header("NPC Settings")]
    public bool helperGivesPlayerTurn = true;

    [Header("Card Setup")]
    public Card cardPrefab;
    public Transform cardContainer;

    [Header("Game Settings")]
    public int pairCount = 25;

    [Header("Card Images")]
    public Sprite[] cardSprites;

    [Header("UI")]
    public TMP_Text turnText;
    public TMP_Text playerScoreText;
    public TMP_Text aiScoreText;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    private List<Card> cards = new List<Card>();

    private Card firstCard;
    private Card secondCard;

    private bool isChecking = false;

    private int playerScore = 0;
    private int aiScore = 0;
    private int matchedPairs = 0;
    private int consecutivePlayerMisses = 0;

    private bool sabotageTriggered = false;

    private enum Turn
    {
        Player,
        AI
    }

    private Turn currentTurn = Turn.Player;


    // =========================================
    // AI MEMORY
    // =========================================

    // Lưu những card AI đã từng nhìn thấy
    // Key   = cardId
    // Value = danh sách Card có cùng hình
    private Dictionary<int, List<Card>> aiMemory =
        new Dictionary<int, List<Card>>();


    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        CreateCards();

        UpdateScoreUI();

        turnText.text = "YOUR TURN";
    }


    // =========================================
    // CREATE CARDS
    // =========================================

    private void CreateCards()
    {
        List<int> cardIds = new List<int>();

        for (int i = 0; i < pairCount; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);

        foreach (int id in cardIds)
        {
            Card newCard = Instantiate(cardPrefab, cardContainer);

            newCard.Setup(id, cardSprites[id]);

            cards.Add(newCard);
        }
    }


    // =========================================
    // SHUFFLE
    // =========================================

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);

            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    // =========================================
    // PLAYER CLICK
    // =========================================

    public void CardClicked(Card card)
    {
        if (currentTurn != Turn.Player)
            return;

        if (isChecking)
            return;

        if (card.IsMatched() || card.IsFlipped())
            return;

        // Player lật card
        card.ShowFront();

        // AI nhìn thấy card này
        RememberCard(card);


        // Card đầu tiên
        if (firstCard == null)
        {
            firstCard = card;
            return;
        }


        // Card thứ hai
        secondCard = card;

        StartCoroutine(CheckPlayerMatch());
    }


    // =========================================
    // CHECK PLAYER MATCH
    // =========================================

    private IEnumerator CheckPlayerMatch()
    {
        isChecking = true;

        yield return new WaitForSeconds(0.7f);


        // PLAYER ĐÚNG
        if (firstCard.cardId == secondCard.cardId)
        {
            firstCard.SetMatched();
            secondCard.SetMatched();

            playerScore++;
            matchedPairs++;

            UpdateScoreUI();

            Debug.Log("PLAYER MATCH!");

            // Hai card này đã ghép rồi
            RemoveFromMemory(firstCard);
            RemoveFromMemory(secondCard);
            yield return StartCoroutine(CheckSabotage());

            firstCard = null;
            secondCard = null;

            isChecking = false;


            if (matchedPairs >= pairCount)
            {
                GameOver();
                yield break;
            }


            // Đúng → Player tiếp tục
            currentTurn = Turn.Player;
            turnText.text = "YOUR TURN";
        }


        // PLAYER SAI
        else
        {
            firstCard.ShowBack();
            secondCard.ShowBack();

            Debug.Log("PLAYER WRONG!");

            consecutivePlayerMisses++;

            Debug.Log(
                "PLAYER WRONG! Consecutive misses = " +
                consecutivePlayerMisses
            );

            firstCard = null;
            secondCard = null;

            // Không gọi Helper ở đây.
            // Player phải chuyển sang AI trước.

            isChecking = false;

            // Sai → AI
            currentTurn = Turn.AI;
            turnText.text = "AI TURN";

            StartCoroutine(AITurn());
        }
    }


    // =========================================
    // AI TURN
    // =========================================

    private IEnumerator AITurn()
    {
        isChecking = true;

        yield return new WaitForSeconds(1f);

        // =====================================
        // AI CHỌN CARD ĐẦU TIÊN
        // =====================================

        Card aiFirstCard = GetBestFirstCard();

        if (aiFirstCard == null)
        {
            isChecking = false;
            yield break;
        }

        aiFirstCard.ShowFront();

        // AI nhớ card này
        RememberCard(aiFirstCard);

        yield return new WaitForSeconds(0.7f);

        // =====================================
        // AI CHỌN CARD THỨ HAI
        // =====================================

        Card aiSecondCard = GetBestSecondCard(aiFirstCard);

        if (aiSecondCard == null)
        {
            aiFirstCard.ShowBack();

            isChecking = false;

            currentTurn = Turn.Player;
            turnText.text = "YOUR TURN";

            yield break;
        }

        aiSecondCard.ShowFront();

        // AI nhớ card này
        RememberCard(aiSecondCard);

        yield return new WaitForSeconds(0.7f);

        // =====================================
        // AI ĐÚNG
        // =====================================

        if (aiFirstCard.cardId == aiSecondCard.cardId)
        {
            aiFirstCard.SetMatched();
            aiSecondCard.SetMatched();

            aiScore++;
            matchedPairs++;

            UpdateScoreUI();

            Debug.Log("AI MATCH!");

            RemoveFromMemory(aiFirstCard);
            RemoveFromMemory(aiSecondCard);

            yield return StartCoroutine(CheckSabotage());

            isChecking = false;

            if (matchedPairs >= pairCount)
            {
                GameOver();
                yield break;
            }

            // Đúng → AI được chơi tiếp
            currentTurn = Turn.AI;
            turnText.text = "AI TURN";

            StartCoroutine(AITurn());
        }

        // =====================================
        // AI SAI
        // =====================================

        else
        {
            aiFirstCard.ShowBack();
            aiSecondCard.ShowBack();

            Debug.Log("AI WRONG!");

            // AI đã hoàn thành lượt.
            // Chỉ tại đây mới kiểm tra Helper.
            yield return StartCoroutine(FinishAITurn());
        }
    }

    // =========================================
    // FINISH AI TURN
    // =========================================

    private IEnumerator FinishAITurn()
    {
        isChecking = true;

        // Helper chỉ xuất hiện sau khi AI đi xong
        // và Player đã sai liên tiếp ít nhất 3 lần.
        if (consecutivePlayerMisses >= 3)
        {
            Debug.Log(
                "HELPER NPC: Player missed " +
                consecutivePlayerMisses +
                " times. Hint after AI turn."
            );

            if (helperNPC != null)
            {
                yield return StartCoroutine(
                    helperNPC.GiveHint(cards)
                );

                // Helper đã dùng lượt gợi ý.
                consecutivePlayerMisses = 0;
            }
            else
            {
                Debug.LogWarning(
                    "HelperNPC is not assigned in AIGameManager."
                );
            }
        }

        // Sau khi Helper rung xong mới trả lượt cho Player.
        currentTurn = Turn.Player;
        turnText.text = "YOUR TURN";
        isChecking = false;
    }

    // =========================================
    // AI MEMORY
    // =========================================

    private void RememberCard(Card card)
    {
        if (card == null)
            return;

        if (card.IsMatched())
            return;


        if (!aiMemory.ContainsKey(card.cardId))
        {
            aiMemory.Add(card.cardId, new List<Card>());
        }


        // Tránh lưu trùng cùng một Card
        if (!aiMemory[card.cardId].Contains(card))
        {
            aiMemory[card.cardId].Add(card);
        }
    }

    // =========================================
    // XÓA CARD ĐÃ MATCH KHỎI MEMORY
    // =========================================

    private void RemoveFromMemory(Card card)
    {
        if (card == null)
            return;

        if (!aiMemory.ContainsKey(card.cardId))
            return;

        aiMemory[card.cardId].Remove(card);

        if (aiMemory[card.cardId].Count == 0)
        {
            aiMemory.Remove(card.cardId);
        }
    }

    // =========================================
    // AI CHỌN CARD ĐẦU TIÊN
    // =========================================

    private Card GetBestFirstCard()
    {
        // Ưu tiên tìm một cặp AI đã biết
        foreach (KeyValuePair<int, List<Card>> pair in aiMemory)
        {
            List<Card> knownCards = pair.Value;

            // Nếu AI biết đủ 2 lá cùng hình
            if (knownCards.Count >= 2)
            {
                Card card1 = null;
                Card card2 = null;

                foreach (Card card in knownCards)
                {
                    if (card.IsMatched())
                        continue;

                    if (card1 == null)
                    {
                        card1 = card;
                    }
                    else if (card != card1)
                    {
                        card2 = card;
                        break;
                    }
                }


                if (card1 != null && card2 != null)
                {
                    return card1;
                }
            }
        }

        // Nếu chưa biết cặp nào → random
        return GetRandomAvailableCard();
    }

    // =========================================
    // AI CHỌN CARD THỨ HAI
    // =========================================

    private Card GetBestSecondCard(Card firstCard)
    {
        // -------------------------------------
        // Trường hợp AI đã biết cặp của firstCard
        // -------------------------------------

        if (aiMemory.ContainsKey(firstCard.cardId))
        {
            foreach (Card card in aiMemory[firstCard.cardId])
            {
                if (card == firstCard)
                    continue;

                if (card.IsMatched())
                    continue;

                if (card.IsFlipped())
                    continue;

                return card;
            }
        }

        // -------------------------------------
        // Chưa biết card còn lại → random
        // -------------------------------------

        return GetRandomAvailableCard(firstCard);
    }

    // =========================================
    // RANDOM CARD
    // =========================================

    private Card GetRandomAvailableCard()
    {
        List<Card> availableCards = new List<Card>();

        foreach (Card card in cards)
        {
            if (!card.IsMatched() && !card.IsFlipped())
            {
                availableCards.Add(card);
            }
        }

        if (availableCards.Count == 0)
            return null;

        int randomIndex = Random.Range(0, availableCards.Count);

        return availableCards[randomIndex];
    }

    private Card GetRandomAvailableCard(Card excludeCard)
    {
        List<Card> availableCards = new List<Card>();

        foreach (Card card in cards)
        {
            if (!card.IsMatched() &&
                !card.IsFlipped() &&
                card != excludeCard)
            {
                availableCards.Add(card);
            }
        }

        if (availableCards.Count == 0)
            return null;

        int randomIndex = Random.Range(0, availableCards.Count);

        return availableCards[randomIndex];
    }

    // =========================================
    // UPDATE SCORE
    // =========================================

    private void UpdateScoreUI()
    {
        playerScoreText.text = playerScore.ToString();

        aiScoreText.text = aiScore.ToString();
    }

    // =========================================
    // GAME OVER
    // =========================================

    private void GameOver()
    {
        Debug.Log("GAME OVER!");

        gameOverManager.ShowResult(playerScore, aiScore);
    }

    private IEnumerator CheckSabotage()
    {
        if (sabotageTriggered)
            yield break;

        // pairCount = 25
        // 50 card → khi 13 cặp đã match thì đạt >= 50% số card
        int sabotageThreshold =
            Mathf.CeilToInt(pairCount / 2f);

        if (matchedPairs < sabotageThreshold)
            yield break;

        sabotageTriggered = true;

        Debug.Log("50% CARDS MATCHED - SABOTAGE NPC!");

        if (sabotageNPC != null)
        {
            isChecking = true;

            yield return StartCoroutine(sabotageNPC.Activate(cards));

            ClearAIMemoryAfterSabotage();
            isChecking = false;
        }
        else
        {
            ClearAIMemoryAfterSabotage();
        }
    }
    
    private void ClearAIMemoryAfterSabotage()
    {
        aiMemory.Clear();

        Debug.Log("AI MEMORY CLEARED!");
    }
}