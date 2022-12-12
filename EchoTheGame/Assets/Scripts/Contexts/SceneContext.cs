using Fusion;
using Project.Echo.Caches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Fusion.NetworkEvents;

namespace Project.Echo.Contexts
{
    [System.Serializable]
    public class SceneContext 
    {
		public ObjectCache ObjectCache;

		[HideInInspector]
		public NetworkRunner Runner;

		[HideInInspector]
		public PlayerRef LocalPlayerRef;
		[HideInInspector]
		public PlayerRef ObservedPlayerRef;
	}
}
