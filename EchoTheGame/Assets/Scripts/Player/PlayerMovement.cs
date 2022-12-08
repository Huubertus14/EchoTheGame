using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;

namespace Project.Echo.Player
{
    public class PlayerMovement : NetworkPositionRotation
    {
     
        public override void FixedUpdateNetwork()
		{
           if (GetInput(out NetworkPlayerMovementData data))
            {
				Vector3 e = transform.position;
                var q = transform.rotation.eulerAngles;
                q.y += data.Rotation + Runner.DeltaTime;
                e += transform.forward * data.Speed * Runner.DeltaTime;

                transform.SetPositionAndRotation(e,Quaternion.Euler(q));
            }
        }
    }
}