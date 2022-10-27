using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;

namespace Project.Echo.Player
{
    public class PlayerMovement : NetworkTransform
    {
        public override void FixedUpdateNetwork()
		{
            if (GetInput(out NetworkPlayerMovementData data))
            {
				Vector3 e = Transform.position;
                e.x += data.Direction.x;
                e.z += data.Direction.y;
                Transform.position = e;
            }
        }
    }
}