using Project.Echo.Player.Controls.Data;
using UnityEngine;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovementController 
    {
        public float _movement = 2.5f;
        public float _rotation = 6f;

        public NetworkPlayerMovementData GetInput()
		{
            NetworkPlayerMovementData data = new();

            if (Input.GetKey(KeyCode.A))
            {
                data.Rotation = -_rotation;
            }
            if (Input.GetKey(KeyCode.D))
            {
                data.Rotation = _rotation;
            }

            if (Input.GetKey(KeyCode.W))
            {
                data.Speed = _movement;
            }
            if (Input.GetKey(KeyCode.S))
            {
                data.Speed = -_movement;
            }

            return data;
		}
    }
}