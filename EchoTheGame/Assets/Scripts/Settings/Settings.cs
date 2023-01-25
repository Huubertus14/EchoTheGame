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

		public static PlayerSettings Player
		{
			get { return _instance._player; }
			set { _instance._player = value; }
		}
		private PlayerSettings _player;

		private void Awake()
		{
			if (_instance!=null)
			{
				Destroy(this);
				return;
			}

			_instance = this;
		}

		private void OnDestroy()
		{
			_instance = null;
		}
	}
}