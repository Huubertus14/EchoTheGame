using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Echo.Player.Controls.Data
{
	public struct NetworkPlayerMovementData : INetworkInput
	{
		//TODO change to int if possible
		public float Speed;
		public Vector2 JoystickRotation;
		public bool IsShooting;
		public bool IsPing;
	}
}
