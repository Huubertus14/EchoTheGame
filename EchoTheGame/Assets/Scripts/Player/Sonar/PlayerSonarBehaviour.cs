using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Project.Echo.Player.Controls.Data;

public class PlayerSonarBehaviour : NetworkBehaviour
{
    private ParticleSystem _sonarParticleSystem;

	private TickTimer _localTickTimer;
	public override void Spawned()
	{
		_sonarParticleSystem = GetComponent<ParticleSystem>();
		
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

	//TODO check if we want to do something with this
	//private void OnParticleCollision(GameObject other)
	//{
		
	//}

	[Rpc(sources:RpcSources.InputAuthority, RpcTargets.All)]
	private void RPC_PlayParticleEffect()
	{
		_localTickTimer = TickTimer.CreateFromSeconds(Runner,0.4f);
		_sonarParticleSystem.Play();
	}
}
