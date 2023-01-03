using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;
using System;
using Project.Echo.Spawner;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovement : NetworkPositionRotation
    {
        private Rigidbody _rigidBody;
        private PlayerNetworkedController _playerNetworkedController;

		public override void Spawned()
		{
			base.Spawned();
            _rigidBody = GetComponent<Rigidbody>();
            _playerNetworkedController = GetComponent<PlayerNetworkedController>();
            _playerNetworkedController.OnRespawned += PlayerDied;
    }

		private void PlayerDied()
		{
            //look for new position
            var pos =PlayerSpawner.GetAvailableSpawnPosition();
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

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			base.Despawned(runner, hasState);
            _playerNetworkedController.OnRespawned -= PlayerDied;
        }
	}
}