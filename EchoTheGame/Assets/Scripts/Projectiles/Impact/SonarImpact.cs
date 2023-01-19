using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Project.Echo.Player.Visuals;

[RequireComponent(typeof(ParticleSystem))]
public class SonarImpact : NetworkBehaviour
{
	ParticleSystem _sonarParticleSystem;
	private List<ParticleCollisionEvent> _hitEvents;
	[SerializeField] private Color _colorToSet; 

	private void Awake()
	{
		_hitEvents = new List<ParticleCollisionEvent>();
		_sonarParticleSystem = GetComponent<ParticleSystem>();
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void RPC_UpdateColor(PlayerRef player)
	{
		if(Runner.TryGetPlayerObject(player, out var nwObject))
		{
			Debug.Log("Get Color", nwObject);
			var pColor =nwObject.GetComponentInChildren<PlayerVisualController>()?.PlayerColor;
			_colorToSet = pColor.Value;

			var main = _sonarParticleSystem.main;
			main.startColor = pColor.Value;
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		_sonarParticleSystem.GetCollisionEvents(other, _hitEvents);

		if (_hitEvents.Count > 0 && other.TryGetComponent<IHitSonarAble>(out var sonar))
		{
			sonar.HitBySonar(_hitEvents[0].intersection, _sonarParticleSystem.main.startColor.color);
		}
	}
}
