using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Bootstrap
{
	public class BootLoader : MonoBehaviour
	{
		AsyncOperation _loadMenu;
		AsyncOperation _loadLoading;

		private void Awake()
		{
			_loadMenu = SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
			_loadLoading=SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
			_loadMenu.completed += OnSceneLoaded;
		}

		private void OnSceneLoaded(AsyncOperation obj)
		{
			_loadMenu.completed-= OnSceneLoaded;
			//Bootstrap loaded
		}
	}
}