using Project.Echo.Player.Controls.Data;
using UnityEngine;
using GameDev.Foundation;
using Project.Echo.Tuning;

namespace Project.Echo.Player.Controls
{
    public class PlayerMovementController 
    {
        public NetworkPlayerMovementData GetInput(SubmarineSettings tuning)
		{
            NetworkPlayerMovementData data = new();

            if (GamePreferences.Held(2))
            {
                data.Rotation = -tuning.rotationPerTick;
            }
            if (GamePreferences.Held(3))
            {
                data.Rotation = tuning.rotationPerTick;
            }

            if (GamePreferences.Held(0))
            {
                data.Speed = tuning.movementSpeed;
            }
            if (GamePreferences.Held(1))
            {
                data.Speed = -tuning.movementSpeed;
            }
            bool gamepadBoost = Input.GetKey(KeyCode.JoystickButton4);
			if (GamePreferences.Held(4) || gamepadBoost)
			{
                data.Speed *= tuning.boostMultiplier;
			}

            float vertical = Input.GetAxisRaw("GamepadVertical"), horizontal = Input.GetAxisRaw("GamepadHorizontal");
            if (Mathf.Abs(vertical) > tuning.gamepadDeadZone && data.Speed == 0)
                data.Speed = vertical * tuning.movementSpeed * (gamepadBoost ? tuning.boostMultiplier : 1f);
            if (Mathf.Abs(horizontal) > tuning.gamepadDeadZone && data.Rotation == 0)
                data.Rotation = horizontal * tuning.rotationPerTick;
            data.IsShooting = GamePreferences.Held(5) || Input.GetKey(KeyCode.JoystickButton0);
            data.IsPing = GamePreferences.Held(6) || Input.GetKey(KeyCode.JoystickButton2);
            return data;
		}
    }
}
