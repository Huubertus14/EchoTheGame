using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovement : NetworkPositionRotation
    {
        private Rigidbody _rigidBody;

		public override void Spawned()
		{
			base.Spawned();
            _rigidBody = GetComponent<Rigidbody>();
    }

		public override void FixedUpdateNetwork()
		{
           if (GetInput(out NetworkPlayerMovementData data))
            {
                _rigidBody.Sleep();
                Vector3 e = transform.position;
                var q = transform.rotation.eulerAngles;
                q.y += data.Rotation + Runner.DeltaTime;
                e += transform.forward * data.Speed * Runner.DeltaTime;
                
                SetEnginePosition(e);
                SetEngineRotation(Quaternion.Euler(q));
            }
        }
    }
}