using System.Collections;
using System.Collections.Generic;
using Project.Echo.Player.Controls.Data;
using Fusion;
using UnityEngine;
using System;
using Project.Echo.Spawner;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovement : NetworkPositionRotation, IRespawnAble
    {
        private Rigidbody _rigidBody;
        private PlayerSonarBehaviour _sonar;
        private bool _touchingObstacle;

		public override void Spawned()
		{
            _rigidBody = GetComponent<Rigidbody>();
            _sonar = GetComponentInChildren<PlayerSonarBehaviour>();
        }

        public void Respawn()
        {
            var pos = PlayerSpawner.GetAvailableSpawnPosition(transform);
            SetEnginePosition(pos);
            _touchingObstacle = false;
        }

        public override void FixedUpdateNetwork()
		{
           if (GetInput(out NetworkPlayerMovementData data))
            {
                _rigidBody.Sleep();
                Vector3 e = transform.position;
                var q = transform.rotation.eulerAngles;
                q.y += data.Rotation + Runner.DeltaTime;
                Vector3 target = e + transform.forward * data.Speed * Runner.DeltaTime;
                var contact = default(Project.Echo.Environment.UnderwaterArena.MovementContact);
                e = Project.Echo.Environment.UnderwaterArena.Instance != null
                    ? Project.Echo.Environment.UnderwaterArena.Instance.Move(e, target, out contact) : target;
                // Swept movement stops before physical contact, so OnCollisionEnter cannot emit this ping.
                if (HasStateAuthority && Runner.IsForward)
                {
                    if (contact.blocked && !_touchingObstacle && _sonar)
                        _sonar.EmitCollision(contact.point, contact.normal);
                    _touchingObstacle = contact.blocked;
                }
                
                SetEnginePosition(e);
                SetEngineRotation(Quaternion.Euler(q));
            }
        }
	}
}
