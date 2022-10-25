using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Menu
{
    public class MenuController : MonoBehaviour
    {
		AsyncOperation _loadOperation;

		private void Awake()
		{
			_loadOperation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
			_loadOperation.completed += OnSceneLoaded;
		}

		private void OnSceneLoaded(AsyncOperation obj)
		{
			_loadOperation.completed -= OnSceneLoaded;
			//Bootstrap loaded
		}
	}
}