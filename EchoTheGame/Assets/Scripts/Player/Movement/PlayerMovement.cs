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

        float maxAngle =12f;
        float rotationSpeed = 60;

        public float SubSpeed = 2.5f;

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

                if (data.JoystickRotation != Vector2.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(new(data.JoystickRotation.x, 0, data.JoystickRotation.y));
                    float step = rotationSpeed * Runner.DeltaTime;
                    Quaternion angles = Quaternion.RotateTowards(transform.rotation, targetRotation, step * maxAngle);
                    SetEngineRotation(angles);
                }

                var currentPosition = transform.position;
                currentPosition += transform.forward * data.Speed *SubSpeed * Runner.DeltaTime;
                currentPosition.y = 0;
                SetEnginePosition(currentPosition);
            }
        }
	}
}