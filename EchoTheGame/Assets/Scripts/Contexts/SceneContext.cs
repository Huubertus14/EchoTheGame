using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkEvents;

namespace Project.Echo.Contexts
{
    [System.Serializable]
    public class SceneContext 
    {
		[HideInInspector]
		public NetworkRunner Runner;

		// Player

		[HideInInspector]
		public PlayerRef LocalPlayerRef;
		[HideInInspector]
		public PlayerRef ObservedPlayerRef;
	}
}
