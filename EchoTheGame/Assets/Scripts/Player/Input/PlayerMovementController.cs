using Project.Echo.Player.Input.Data;
using System.Collections;
using UnityEngine;

namespace Project.Echo.Player.Input
{
    public class PlayerMovementController : MonoBehaviour
    {
        public NetworkPlayerMovementData GetInput()
		{
            NetworkPlayerMovementData data = new();
            if (Input.GetKeyDown("space"))
            {
                print("space key was pressed");
            }
            return data;
		}
    }
}