using Project.Echo.Player.Controls.Data;
using UnityEngine;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovementController 
    {
        public NetworkPlayerMovementData GetInput()
		{
            NetworkPlayerMovementData data = new();
            if (Input.GetKey(KeyCode.A))
            {
            data.Direction.x = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            data.Direction.x = -1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            data.Direction.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            data.Direction.y = -1;
        }

        return data;
		}
    }
}