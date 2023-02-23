using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;
using System;
using Project.Echo.Spawner;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovement : NetworkPositionRotation, IHiddenRespawnAble
    {
        private Rigidbody _rigidBody;

		public override void Spawned()
		{
            _rigidBody = GetComponent<Rigidbody>();
        }

        public void Respawn()
        {
            var pos = PlayerSpawner.GetAvailableSpawnPosition();
            SetEnginePosition(pos);
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