using UnityEngine;

public enum GameMode
{
    Move,
    Time
}

[System.Serializable]
public class LevelConfig
{
    [Header("Level")]
    public int levelNumber;

    [Header("Cards")]
    public int cardCount;

    [Header("Game Mode")]
    public GameMode gameMode;

    [Header("Limit")]
    public int moveLimit;
    public float timeLimit;

    [Header("Match")]
    public int cardsPerMatch;
}