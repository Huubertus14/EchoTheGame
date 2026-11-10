using System;
using System.Collections.Generic;
using Fusion;
using Project.Echo.Loading;
using Project.Echo.Setting;
using Project.Echo.Tuning;
using UnityEngine;

/// <summary>One coordinator on the host's player owns lobby and round transitions.</summary>
public class NetworkedPlayerGameModeController : NetworkBehaviour
{
    public bool SkipInit;
    [Networked] public NetworkBool IsCoordinator { get; private set; }
    [Networked] public NetworkBool Ready { get; private set; }
    [Networked] public int Phase { get; private set; }
    [Networked] public NetworkString<_32> RoundId { get; private set; }
    [Networked] private TickTimer Countdown { get; set; }
    [Networked] private TickTimer RoundTimer { get; set; }
    [Networked] public float SecondsLeft { get; private set; }
    [Networked] public float CountdownLeft { get; private set; }
    public static NetworkedPlayerGameModeController Coordinator { get; private set; }
    public int MaxPlayers => Runner.GameMode != GameMode.Single && Runner.SessionInfo.IsValid && Runner.SessionInfo.MaxPlayers > 0
        ? Runner.SessionInfo.MaxPlayers
        : MatchRules.FreeForAll.MaxPlayers;
    private readonly List<PlayerScoreboardController> players = new();
    private MatchStatus? restored;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => Coordinator = null;

    public override void Spawned()
    {
        if (HasStateAuthority && HasInputAuthority)
        {
            IsCoordinator = true;
            if (restored.HasValue)
            {
                var match = restored.Value;
                Phase = match.GameOver ? 3 : match.GameStarted ? 2 : 0;
                RoundId = match.RoundId ?? "";
                SecondsLeft = match.TimeLeft;
                RoundTimer = match.GameStarted && match.TimeLeft > 0 ? TickTimer.CreateFromSeconds(Runner, match.TimeLeft) : TickTimer.None;
            }
            if (!SkipInit) Ready = false;
        }
        LoadScreenController.Hide();
        EchoFoundation.IsConnecting = false;
    }
    public override void Render()
    {
        if (!IsCoordinator || MatchManager.Instance == null) return;
        Coordinator = this;
        MatchManager.Instance.IsGameStarted = Phase == 2;
        MatchManager.Instance.IsGameOver = Phase == 3;
        MatchManager.Instance.TimeLeft = SecondsLeft;
        MatchManager.Instance.PreGameTime = CountdownLeft;
        MatchManager.Instance.RoundId = RoundId.ToString();
        EchoFoundation.MatchId = RoundId.ToString();
    }
    public bool AllReady()
    {
        PlayerScoreboardController.GetSortedPlayers(players);
        int count = 0;
        foreach (var player in players)
        {
            if (!player.Object || !player.Object.IsValid) continue;
            var lobby = player.GetComponent<NetworkedPlayerGameModeController>();
            if (lobby == null || !lobby.Ready) return false;
            count++;
        }
        int connected = 0;
        foreach (var player in Runner.ActivePlayers) connected++;
        return count >= MatchRules.FreeForAll.MinPlayers && count == connected;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetReady(bool value)
    {
        if (Coordinator == null || Coordinator.Phase == 0) Ready = value;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_StartRound()
    {
        if (!IsCoordinator || Phase != 0 || !AllReady()) return;
        RoundId = Guid.NewGuid().ToString("N");
        CountdownLeft = MatchRules.FreeForAll.countdownSeconds;
        Phase = 1;
        Countdown = TickTimer.CreateFromSeconds(Runner, Mathf.Max(Runner.DeltaTime, CountdownLeft));
        Runner.SessionInfo.IsOpen = false;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_ReturnToLobby()
    {
        if (!IsCoordinator || Phase != 3) return;
        PlayerScoreboardController.GetSortedPlayers(players);
        foreach (var player in players)
        {
            player.ResetRound();
            player.GetComponent<Project.Echo.Projectiles.ProjectileManager>().ResetRound();
            player.GetComponent<NetworkedPlayerGameModeController>().Ready = false;
            player.GetComponent<Project.Echo.Player.PlayerNetworkedController>().ResetForRound();
        }
        Phase = 0; Countdown = TickTimer.None; RoundTimer = TickTimer.None;
        float timeLimit = MatchRules.FreeForAll.timeLimitSeconds;
        SecondsLeft = timeLimit > 0f ? timeLimit : -1f;
        CountdownLeft = 0;
        Runner.SessionInfo.IsOpen = true;
    }
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsCoordinator) return;
        Coordinator = this;
        if (Phase == 1)
        {
            if (!AllReady()) { Phase = 0; Countdown = TickTimer.None; Runner.SessionInfo.IsOpen = true; return; }
            CountdownLeft = Mathf.Max(0, Countdown.RemainingTime(Runner) ?? 0);
            if (Countdown.Expired(Runner))
            {
                Phase = 2; Countdown = TickTimer.None;
                float timeLimit = MatchRules.FreeForAll.timeLimitSeconds;
                SecondsLeft = timeLimit > 0f ? timeLimit : -1f;
                RoundTimer = timeLimit > 0f
                    ? TickTimer.CreateFromSeconds(Runner, Mathf.Max(Runner.DeltaTime, timeLimit))
                    : TickTimer.None;
            }
        }
        else if (Phase == 2)
        {
            if (RoundTimer.IsRunning) SecondsLeft = Mathf.Max(0, RoundTimer.RemainingTime(Runner) ?? 0);
            PlayerScoreboardController.GetSortedPlayers(players);
            bool ended = RoundTimer.Expired(Runner);
            foreach (var player in players) ended |= MatchRules.FreeForAll.HasReachedLimit(player.Score, player.Kills);
            if (ended) { Phase = 3; RoundTimer = TickTimer.None; }
        }
    }
    public void SetMatchValues(MatchStatus matchData) => restored = matchData;
    public override void Despawned(NetworkRunner runner, bool hasState) { if (Coordinator == this) Coordinator = null; }
}
