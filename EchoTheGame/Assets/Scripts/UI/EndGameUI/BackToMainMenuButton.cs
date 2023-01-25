using Project.Echo.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BackToMainMenuButton : MonoBehaviour
{
	private AsyncOperation _loadMenuScene;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		PlayerNetworkedController.LocalPlayer.Runner.Shutdown();
		_loadMenuScene = SceneManager.LoadSceneAsync("MainMenu");
		_loadMenuScene.completed += OnLoadDone;
	}

	private void OnLoadDone(AsyncOperation obj)
	{
		_loadMenuScene.completed -= OnLoadDone;
	}

	private void OnDestroy()
	{
		GetComponent<Button>().onClick.RemoveListener(OnClick);
	}
}
