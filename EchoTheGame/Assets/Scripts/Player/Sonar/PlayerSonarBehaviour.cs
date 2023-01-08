using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Player.Controls.Data;

public class PlayerSonarBehaviour : NetworkBehaviour
{
    private ParticleSystem _sonarParticleSystem;

	private TickTimer _localTickTimer;

	private List<ParticleCollisionEvent> _hitEvents;	

	public override void Spawned()
	{
		_sonarParticleSystem = GetComponent<ParticleSystem>();
		_hitEvents = new List<ParticleCollisionEvent>();
		_localTickTimer = TickTimer.None;

		ParticleSystem.MainModule particleSettings = _sonarParticleSystem.main;
		if(HasInputAuthority)
		{
			particleSettings.startColor = Color.blue;
		}
		else
		{
			particleSettings.startColor = Color.red;
		}
	}

	public override void FixedUpdateNetwork()
	{
		if(GetInput(out NetworkPlayerMovementData data) && _localTickTimer.ExpiredOrNotRunning(Runner))
		{
			if(data.IsPing)
			{
				RPC_PlayParticleEffect();
			}
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		_sonarParticleSystem.GetCollisionEvents(other, _hitEvents);

		if(other.TryGetComponent<IHitSonarAble>(out var sonar))
		{
			sonar.HitBySonar(_hitEvents.FirstOrDefault().intersection);
		}
	}

	[Rpc(sources:RpcSources.InputAuthority, RpcTargets.All)]
	private void RPC_PlayParticleEffect()
	{
		_localTickTimer = TickTimer.CreateFromSeconds(Runner,0.4f);
		_sonarParticleSystem.Play();
	}
}
