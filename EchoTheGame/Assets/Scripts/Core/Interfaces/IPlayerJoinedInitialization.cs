using System.Threading.Tasks;
using Fusion;

public interface IPlayerJoinedInitialization 
{
    public Task Init(NetworkRunner runner);
}
