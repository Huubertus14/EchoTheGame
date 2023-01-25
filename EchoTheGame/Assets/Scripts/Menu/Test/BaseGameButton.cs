using UnityEngine;
using UnityEngine.UI;

namespace Project.Echo.Menu.Test
{
	[RequireComponent(typeof(Button))]
	public abstract class BaseGameButton : MonoBehaviour
	{
		private Button _button;

		private void Awake()
		{
			_button = gameObject.GetComponent<Button>();
			_button.onClick.AddListener(OnClick);
		}

		protected virtual void OnClick()
		{
			Loading.LoadScreenController.Show();
		}
		

		private void OnDestroy()
		{
			_button.onClick.RemoveListener(OnClick);
		}
	}
}