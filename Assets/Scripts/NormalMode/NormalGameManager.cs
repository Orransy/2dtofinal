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

    [Header("Boom")]
    public Sprite boomSprite;

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

            newCard.Setup(id, cardSprites[id], false);

            cards.Add(newCard);
        }

        // Tạo Boom
        if (currentConfig.hasBoom)
        {
            for (int i = 0; i < currentConfig.boomCount; i++)
            {
                Card boomCard = Instantiate(cardPrefab, cardContainer);

                boomCard.Setup(-1, boomSprite, true);

                cards.Add(boomCard);
            }
        }

        // Trộn toàn bộ card lần đầu
        ShuffleCardsInContainer();

        Debug.Log(
            "Đã tạo " + cards.Count + " cards. Card thường: " +
            currentConfig.cardCount + " | Boom: " +
            (currentConfig.hasBoom ? currentConfig.boomCount : 0)
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

        // BOOM
        if (card.IsBoom())
        {
            StartCoroutine(ActivateBoomRoutine(card));
            return;
        }

        card.ShowFront();

        selectedCards.Add(card);

        // Chỉ kiểm tra khi đã chọn đủ số card của nhóm
        if (selectedCards.Count >= currentConfig.cardsPerMatch)
        {
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator ActivateBoomRoutine(Card boomCard)
    {
        isChecking = true; // Khóa click trong lúc xử lý

        // 1. Mở mặt Boom tại vị trí vừa bấm
        boomCard.ShowFront();

        // 2. Chờ 0.8s để người chơi nhìn rõ mặt Boom
        yield return new WaitForSeconds(0.8f);

        // 3. Úp LẠI các thẻ thường chọn dở trong lượt này (CHỈ úp thẻ thường CHƯA MATCH)
        foreach (Card card in selectedCards)
        {
            if (card != null && !card.IsMatched() && !card.IsBoom() && card != boomCard)
            {
                card.ShowBack();
            }
        }
        selectedCards.Clear();

        // LƯU Ý: KHÔNG gọi boomCard.ShowBack() nữa -> Thẻ Boom giữ nguyên mặt ngửa vĩnh viễn

        yield return new WaitForSeconds(0.2f);

        // 4. Trộn CHỈ những thẻ ĐANG ÚP trên bàn chơi
        yield return StartCoroutine(ShuffleFaceDownCardsRoutine());

        // 5. Trừ lượt / Cập nhật UI trong Move Mode
        if (currentConfig.gameMode == GameMode.Move)
        {
            currentMoves += 2;

            if (currentMoves > currentConfig.moveLimit)
            {
                currentMoves = currentConfig.moveLimit;
            }

            UpdateUI();

            if (currentMoves >= currentConfig.moveLimit)
            {
                GameOver();
                yield break;
            }
        }

        isChecking = false; // Mở khóa cho người chơi tiếp tục chơi
    }

    private IEnumerator ShuffleFaceDownCardsRoutine()
    {
        List<Card> faceDownCards = new List<Card>();
        List<Vector3> originalPositions = new List<Vector3>();
        List<int> validIndices = new List<int>();

        // 1. Lấy danh sách thẻ đang úp và tọa độ AnchoredPosition tương ứng
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            Card card = child.GetComponent<Card>();

            if (card != null && !card.IsFlipped() && !card.IsMatched() && !card.IsBoom())
            {
                faceDownCards.Add(card);
                validIndices.Add(i);
                originalPositions.Add(child.GetComponent<RectTransform>().anchoredPosition);
            }
        }

        if (faceDownCards.Count <= 1)
            yield break;

        // 2. Xáo trộn danh sách thẻ úp
        for (int i = faceDownCards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Card tempCard = faceDownCards[i];
            faceDownCards[i] = faceDownCards[randomIndex];
            faceDownCards[randomIndex] = tempCard;
        }

        // 3. Chạy hiệu ứng di chuyển + xoay bài đến vị trí mới
        List<Coroutine> moveCoroutines = new List<Coroutine>();
        for (int i = 0; i < faceDownCards.Count; i++)
        {
            // Di chuyển thẻ đã xáo trộn đến tọa độ ban đầu của vị trí mới
            moveCoroutines.Add(StartCoroutine(faceDownCards[i].MoveToPosition(originalPositions[i])));
        }

        // Chờ tất cả thẻ di chuyển xong
        yield return new WaitForSeconds(0.5f);

        // 4. Cập nhật SiblingIndex để GridLayoutGroup/UI quản lý chuẩn xác
        for (int i = 0; i < faceDownCards.Count; i++)
        {
            faceDownCards[i].transform.SetSiblingIndex(validIndices[i]);
        }
    }

    private void ShuffleCardsInContainer()
    {
        List<Transform> children = new List<Transform>();

        for (int i = 0; i < cardContainer.childCount; i++)
        {
            children.Add(cardContainer.GetChild(i));
        }

        for (int i = 0; i < children.Count; i++)
        {
            int randomIndex = Random.Range(i, children.Count);

            Transform temp = children[i];

            children[i] = children[randomIndex];

            children[randomIndex] = temp;
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
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