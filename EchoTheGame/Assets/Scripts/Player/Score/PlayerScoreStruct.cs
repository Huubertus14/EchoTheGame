using Fusion;

public struct PlayerScoreStruct : INetworkStruct
{
    public uint Score;
    public uint Kills;
    public uint Deaths;
}
