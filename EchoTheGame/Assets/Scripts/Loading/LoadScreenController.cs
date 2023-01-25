using UnityEngine;
using TMPro;

namespace Project.Echo.Loading
{
    public class LoadScreenController : MonoBehaviour
    {
        private static LoadScreenController _instance;

		[SerializeField]private TextMeshProUGUI _loadingScreenText;

        private void Awake()
		{
			if (_instance== null)
			{
                _instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
			_loadingScreenText.text = "";
			Hide();
		}

        public static void Show()
		{
            _instance.gameObject.SetActive(true);
		}

		public static void Hide()
		{
			_instance.gameObject.SetActive(false);
		}
        
        public static void SetLoadingText(string message)
		{
            Debug.Log(message);
			_instance._loadingScreenText.text = message;
		}
    }
}