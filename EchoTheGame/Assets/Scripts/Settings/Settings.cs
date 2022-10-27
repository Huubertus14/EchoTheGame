using Project.Echo.Setting.Session;
using UnityEngine;

namespace Project.Echo.Setting
{
    public class Settings : MonoBehaviour
    {
        private static Settings _instance;

		public static SessionSettings Session {
			get { return _instance._session; }
			set { _instance._session = value; }
		}
		private SessionSettings _session;

		private void Awake()
		{
			if (_instance!=null)
			{
				Destroy(this);
				return;
			}

			_instance = this;
			DontDestroyOnLoad(this);
		}

		private void OnDestroy()
		{
			_instance = null;
		}
	}
}