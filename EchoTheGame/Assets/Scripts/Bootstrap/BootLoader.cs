using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Bootstrap
{
	public class BootLoader : MonoBehaviour
	{
		AsyncOperation _loadOperation;

		private void Awake()
		{
			_loadOperation = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
			_loadOperation.completed += OnSceneLoaded;
		}

		private void OnSceneLoaded(AsyncOperation obj)
		{
			_loadOperation.completed-= OnSceneLoaded;
			//Bootstrap loaded
		}
	}
}