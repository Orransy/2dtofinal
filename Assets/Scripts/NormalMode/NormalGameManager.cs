using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NormalGameManager : MonoBehaviour
{
    public static NormalGameManager Instance;

    [Header("Level Settings")]
    public LevelConfig[] levels;

    [Header("Current Level")]
    public int currentLevel = 1;

    [Header("Card Setup")]
    public Card cardPrefab;
    public Transform cardContainer;

    [Header("Card Images")]
    public Sprite[] cardSprites;

    [Header("UI Containers")]
    public GameObject moveContainer;
    public GameObject timeContainer;

    [Header("UI Text")]
    public TMP_Text moveText;
    public TMP_Text timeText;
    public TMP_Text levelText;

    [Header("Game Over")]
    public GameOverManager gameOverManager;

    private LevelConfig currentConfig;

    private List<Card> cards = new List<Card>();
    private List<Card> selectedCards = new List<Card>();

    private bool isChecking = false;

    private int currentMoves = 0;
    private float currentTime = 0f;

    private int matchedGroups = 0;
    private bool gameFinished = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentLevel = PlayerPrefs.GetInt("SelectedLevel", 1);

        LoadLevel(currentLevel);
    }

    private void LoadLevel(int level)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("Chưa có LevelConfig!");
            return;
        }

        int index = level - 1;

        if (index < 0 || index >= levels.Length)
        {
            Debug.LogError("Level không tồn tại: " + level);
            return;
        }

        currentConfig = levels[index];

        currentMoves = 0;
        currentTime = currentConfig.timeLimit;
        matchedGroups = 0;
        gameFinished = false;

        selectedCards.Clear();
        cards.Clear();

        UpdateUI();

        Debug.Log(
            "Level " + currentConfig.levelNumber +
            " | Cards: " + currentConfig.cardCount +
            " | Mode: " + currentConfig.gameMode +
            " | Match: " + currentConfig.cardsPerMatch
        );

        CreateCards();
    }

    private void CreateCards()
    {
        List<int> cardIds = new List<int>();

        int totalGroups =
            currentConfig.cardCount / currentConfig.cardsPerMatch;

        for (int id = 0; id < totalGroups; id++)
        {
            for (int i = 0; i < currentConfig.cardsPerMatch; i++)
            {
                cardIds.Add(id);
            }
        }

        Shuffle(cardIds);

        foreach (int id in cardIds)
        {
            Card newCard = Instantiate(cardPrefab, cardContainer);

            newCard.Setup(id, cardSprites[id]);

            cards.Add(newCard);
        }

        Debug.Log(
            "Đã tạo " + cards.Count +
            " cards, chia thành " + totalGroups + " nhóm."
        );
    }

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

    public void CardClicked(Card card)
    {
        if (gameFinished)
            return;

        if (isChecking)
            return;

        if (card.IsMatched())
            return;

        if (card.IsFlipped())
            return;

        card.ShowFront();

        selectedCards.Add(card);

        // Chỉ kiểm tra khi đã chọn đủ số card của nhóm
        if (selectedCards.Count >= currentConfig.cardsPerMatch)
        {
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        isChecking = true;

        // Cho người chơi nhìn thấy các card
        yield return new WaitForSeconds(0.7f);

        bool isMatch = true;

        int firstId = selectedCards[0].cardId;

        // Kiểm tra tất cả card có cùng ID không
        foreach (Card card in selectedCards)
        {
            if (card.cardId != firstId)
            {
                isMatch = false;
                break;
            }
        }

        if (isMatch)
        {
            foreach (Card card in selectedCards)
            {
                card.SetMatched();
            }

            matchedGroups++;

            Debug.Log(
                "MATCH! " +
                matchedGroups + " / " +
                (currentConfig.cardCount / currentConfig.cardsPerMatch)
            );

            selectedCards.Clear();

            int totalGroups =
                currentConfig.cardCount / currentConfig.cardsPerMatch;

            // Kiểm tra Win trước Game Over
            if (matchedGroups >= totalGroups)
            {
                WinGame();
                yield break;
            }
        }
        else
        {
            foreach (Card card in selectedCards)
            {
                card.ShowBack();
            }

            Debug.Log("WRONG!");

            selectedCards.Clear();
        }

        // =========================
        // MOVE MODE
        // =========================

        // Chỉ tính Move sau khi đã hoàn thành
        // việc kiểm tra một nhóm card
        if (currentConfig.gameMode == GameMode.Move)
        {
            currentMoves++;

            UpdateUI();

            Debug.Log(
                "Move: " +
                currentMoves +
                " / " +
                currentConfig.moveLimit
            );

            // Hết lượt sau khi lượt cuối đã hoàn thành
            if (currentMoves >= currentConfig.moveLimit)
            {
                GameOver();
                yield break;
            }
        }

        isChecking = false;
    }

    private void Update()
    {
        if (gameFinished)
            return;

        if (currentConfig == null)
            return;

        // Move mode không chạy timer
        if (currentConfig.gameMode != GameMode.Time)
            return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            UpdateUI();

            GameOver();

            return;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentConfig == null)
            return;

        // =========================
        // LEVEL
        // =========================

        levelText.gameObject.SetActive(true);

        levelText.text =
            "Level " + currentConfig.levelNumber;


        // =========================
        // MOVE MODE
        // =========================

        if (currentConfig.gameMode == GameMode.Move)
        {
            moveContainer.SetActive(true);
            timeContainer.SetActive(false);

            int movesRemaining =
                currentConfig.moveLimit - currentMoves;

            if (movesRemaining < 0)
                movesRemaining = 0;

            moveText.text =
                "MOVES: " + movesRemaining;
        }


        // =========================
        // TIME MODE
        // =========================

        else
        {
            moveContainer.SetActive(false);
            timeContainer.SetActive(true);

            int totalSeconds =
                Mathf.CeilToInt(currentTime);

            int minutes =
                totalSeconds / 60;

            int seconds =
                totalSeconds % 60;

            timeText.text =
                "TIME: " +
                minutes.ToString("0") +
                ":" +
                seconds.ToString("00");
        }
    }

    private void WinGame()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        PlayerPrefs.SetInt(
            "LevelCompleted_" + currentLevel,
            1
        );

        PlayerPrefs.Save();

        Debug.Log("YOU WIN!");

        gameOverManager.ShowWin();
    }

    private void GameOver()
    {
        if (gameFinished)
            return;

        gameFinished = true;

        Debug.Log("GAME OVER!");

        gameOverManager.ShowGameOver();
    }
}