using Cysharp.Threading.Tasks;
using Project.Echo.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

namespace Project.Echo.Player.Visuals
{
    public class PlayerVisualController : SimulationBehaviour , IAfterSpawned
    {
		private Renderer[] _renderers;

		public void AfterSpawned()
		{
			//_renderers = GetComponentsInChildren<Renderer>();
//
			//if (Runner.GetPlayerObject(Runner.LocalPlayer).HasInputAuthority)
		//	{
			//	Debug.Log("Set to blue");
			//	SetColor(Color.blue);
			//}

			//Debug.Log("Set to red");
		//	SetColor(Color.red);
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