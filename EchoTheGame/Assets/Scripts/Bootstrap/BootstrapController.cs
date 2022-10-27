using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Echo.Bootstrap
{
    public class BootstrapController : MonoBehaviour
    {
		AsyncOperation _menuLoading;

		private void Awake()
		{
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
			SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
		}
	}
}