using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Project.Echo.Setting.Session
{
    public class SessionSettings 
    {
        public readonly string LobbyName;
        public readonly GameMode Mode;

		public SessionSettings(string lobbyName, GameMode mode)
		{
			Mode = mode;
			LobbyName = lobbyName;
		}
	}
}