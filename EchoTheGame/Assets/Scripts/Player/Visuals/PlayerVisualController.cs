using Cysharp.Threading.Tasks;
using Project.Echo.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : NetworkBehaviour 
    {
		private Renderer[] _renderers;

		[Networked]
		private Color _color { get; set; }

		public override void Spawned()
		{
			_renderers = GetComponentsInChildren<Renderer>();

			if (Object.HasInputAuthority)
			{
				_color = Color.blue;
			}
			else
			{
				_color = Color.red;
			}
			SetColor(_color);
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