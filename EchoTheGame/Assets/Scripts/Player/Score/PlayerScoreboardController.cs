using System;
using System.Collections.Generic;
using Fusion;
using Project.Echo.Setting;
using UnityEngine;

public class PlayerScoreboardController : NetworkBehaviour
{
    private static readonly HashSet<PlayerScoreboardController> Players = new();
    public static Action<int, int> ScoreChanged;
    [Networked] private int _matchKils { get; set; }
    [Networked] private int _matchDeaths { get; set; }
    [Networked] private int _matchScore { get; set; }
    [Networked] private NetworkString<_32> _playerName { get; set; }
    [Networked] public int PingMilliseconds { get; private set; } = -1;
    private TickTimer _pingTimer;
    private bool _isNameMessageSend;
    public bool SkipInitialization;
    public string GetPlayerName => _playerName.ToString();
    public int Score => _matchScore;
    public int Kills => _matchKils;
    public int Deaths => _matchDeaths;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPlayers() => Players.Clear();

    // Read replicated snapshots so late joiners get existing scores. Object identity
    // keeps two players with the same display name in separate rows.
    public static void GetSortedPlayers(List<PlayerScoreboardController> result)
    {
        result.Clear();
        foreach (var player in Players)
            if (player && player.Object && player.Object.IsValid) result.Add(player);
        result.Sort((a, b) =>
        {
            int order = b.Score.CompareTo(a.Score);
            if (order == 0) order = b.Kills.CompareTo(a.Kills);
            if (order == 0) order = a.Deaths.CompareTo(b.Deaths);
            return order != 0 ? order : a.Object.InputAuthority.RawEncoded.CompareTo(b.Object.InputAuthority.RawEncoded);
        });
    }

    public override void Spawned()
    {
        Players.Add(this);
        if (HasInputAuthority && !SkipInitialization) RPC_SendNameToHost(Settings.Player.PlayerName);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SendNameToHost(string name)
    {
        _playerName = string.IsNullOrWhiteSpace(name) ? "Unknown" : name;
        if (_isNameMessageSend) return;
        GetComponent<NetworkedKillFeedController>()?.SetKillFeed(GetPlayerName, "Joined the game");
        _isNameMessageSend = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !_pingTimer.ExpiredOrNotRunning(Runner)) return;
        _pingTimer = TickTimer.CreateFromSeconds(Runner, 1);
        var player = Object.InputAuthority;
        if (player.IsNone) { PingMilliseconds = -1; return; }
        double rtt = Runner.GetPlayerRtt(player);
        PingMilliseconds = rtt > 0 ? Mathf.RoundToInt((float)rtt * 1000) :
            player == Runner.LocalPlayer ? 0 : -1;
    }

    public void AddScore(int score)
    {
        if (!HasStateAuthority) return;
        _matchScore += score;
        ScoreChanged?.Invoke(_matchScore, _matchKils);
    }
    public void AddKills(int kills = 1)
    {
        if (!HasStateAuthority) return;
        _matchKils += kills;
        ScoreChanged?.Invoke(_matchScore, _matchKils);
    }
    public void AddDeath() { if (HasStateAuthority) _matchDeaths++; }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        Players.Remove(this);
        if (hasState && MatchManager.Instance && !MatchManager.Instance.IsGameOver)
            GetComponent<NetworkedKillFeedController>()?.SetKillFeed(GetPlayerName, "left the game");
    }
    private void OnDestroy() => Players.Remove(this);
}
