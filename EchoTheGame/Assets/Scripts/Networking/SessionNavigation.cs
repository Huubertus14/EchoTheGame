using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Networking
{
    /// <summary>Both exit routes wait for runner shutdown before unloading the game scene.</summary>
    public static class SessionNavigation
    {
        private static Task returning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => returning = null;

        public static Task ReturnToMenuAsync() => returning ??= ReturnInternalAsync();

        private static async Task ReturnInternalAsync()
        {
            try
            {
                if (NetworkController.Instance != null) await NetworkController.Instance.LeaveGame();
                var operation = SceneManager.LoadSceneAsync("MainMenu");
                while (operation != null && !operation.isDone) await Task.Yield();
            }
            finally { returning = null; }
        }
    }
}
