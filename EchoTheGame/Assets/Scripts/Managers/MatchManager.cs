using UnityEngine;

public struct MatchStatus
{
    public float TimeLeft;
    public float PreGameTime;
    public bool GameOver;
    public bool GameStarted;
}

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;
    public bool IsGameStarted;
    public bool IsGameOver;
    public float TimeLeft { get; set; }
    public float PreGameTime { get; set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public MatchStatus GetMatchData() => new MatchStatus
    {
        TimeLeft = TimeLeft,
        PreGameTime = PreGameTime,
        GameStarted = IsGameStarted,
        GameOver = IsGameOver
    };
    // Results stay in the hold-to-view scoreboard; Escape still returns to the menu.
    public void ShowEndScreen() => IsGameOver = true;
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
