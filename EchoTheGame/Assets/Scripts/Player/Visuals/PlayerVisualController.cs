using UnityEngine;
using Fusion;
using System;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : NetworkBehaviour 
    {
		private Renderer[] _renderers;

		public Color PlayerColor { get; set; }

		public Action<Color> ColorSet { get; set; }

		public override void Spawned()
		{
			_renderers = GetComponentsInChildren<Renderer>();

			if (Object.HasInputAuthority)
			{
				PlayerColor = Color.blue;
			}
			else
			{
				PlayerColor = Color.red;
			}
			SetColor(PlayerColor);
			ColorSet?.Invoke(PlayerColor);
		}

		private void SetColor(Color col)
		{
			foreach (var ren in _renderers)
			{
				ren.material.color = col;
			}
		}
	}
}