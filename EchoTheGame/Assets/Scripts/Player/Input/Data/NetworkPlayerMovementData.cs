using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Echo.Player.Input.Data
{
	public struct NetworkPlayerMovementData : INetworkInput
	{
		public Vector2 Direction;
	}
}
