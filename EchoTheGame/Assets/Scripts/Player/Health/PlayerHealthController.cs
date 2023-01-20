using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Project.Echo.Projectiles.Enums;
using Project.Echo.Player.Visuals;
using Project.Echo.Player;
using System;
using Cysharp.Threading.Tasks;

public class PlayerHealthController : NetworkBehaviour, IRespawnAble
{
	[SerializeField] private GameObject _healthBarPrefab;

	[Networked(OnChanged = nameof(UpdateHealthBar))]
	public int PlayerHealth { get; set; }

	[Networked(OnChanged =nameof(OnAliveChanged))]
	public NetworkBool Alive {get;set;}

	private PlayerRef _lastHitByPlayer;

	private int _maxHealth = 100;
	private HealthBarPositionBehaviour _healthBarSlider;
	private PlayerNetworkedController _playerNetworkController;
	private NetworkedKillFeedController _killFeedController;
	[SerializeField]private PlayerScoreboardController _playerScoreBoard;
	public bool SkipInit = false;
	
	public override void Spawned()
	{
		base.Spawned();

		if (!SkipInit)
		{
			PlayerHealth = _maxHealth;
		}

		_killFeedController = GetComponent<NetworkedKillFeedController>();
		_playerScoreBoard = GetComponent<PlayerScoreboardController>();
		_playerNetworkController = GetComponentInChildren<PlayerNetworkedController>();
		_healthBarSlider = Instantiate(_healthBarPrefab).GetComponentInChildren<HealthBarPositionBehaviour>();
		_healthBarSlider.Init(GameManager.GetConnectionToken(),transform, _maxHealth);
		Alive = true;
	}

	public void Respawn()
	{
		Alive = true;
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	private void RPC_OnRespawned()
	{
		PlayerHealth = _maxHealth;
		_healthBarSlider.gameObject.SetActive(true);
		_healthBarSlider.UpdateSlider(PlayerHealth);
	}

	public void HitPlayer(int damage, PlayerRef hitByPlayer)
	{
		if (!HasStateAuthority || !Alive) return;

			PlayerHealth -= damage;
			_lastHitByPlayer = hitByPlayer;
	}

	private void UpdateHealth()
{
		_healthBarSlider.UpdateSlider(PlayerHealth);

		if (PlayerHealth < 0)
		{
			Alive = false;
			if (HasStateAuthority)
			{
				//Add kill to last hit player
				if (Runner.TryGetPlayerObject(_lastHitByPlayer, out NetworkObject playerNetworkObject))
				{
					var playerScoreboard =playerNetworkObject.GetComponent<PlayerScoreboardController>();
					playerScoreboard.AddScore(50);
					playerScoreboard.AddKills(1);
					_killFeedController.SetKillFeed(playerScoreboard.GetPlayerName, $"Killed {_playerScoreBoard.GetPlayerName}");
				}
				_playerScoreBoard.AddDeath();
			}
		}
	}

	public static void UpdateHealthBar(Changed<PlayerHealthController> changed)
	{
		var thisObject = changed.Behaviour;
		thisObject.UpdateHealth();
	}

	private static void OnAliveChanged(Changed<PlayerHealthController> changed)
	{
		var newValue = changed.Behaviour.Alive;
		if (changed.Behaviour.HasStateAuthority) 
		{ 
			if (newValue)
			{
				changed.Behaviour.RPC_OnRespawned();
			}
			else 
			{
			
				changed.Behaviour.RPC_PlayerDied();
			}
		}
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	private void RPC_PlayerDied()
	{
		_healthBarSlider.gameObject.SetActive(false);
		_playerNetworkController.DisablePlayer();
		_playerNetworkController.RespawnPlayer(2.5f);
	}
	
	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		_healthBarSlider.SetPosition();
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (_healthBarSlider != null)
		{
			DestroyImmediate(_healthBarSlider.gameObject);
		}
		else
		{
			//Not possible // Find healthbar
			var healthbars = FindObjectsOfType<HealthBarPositionBehaviour>();
			foreach (var bar in healthbars)
			{
				if (bar.connectionToken == GameManager.GetConnectionToken())
				{
					Destroy(bar);
					break;
				}
			}
		}
	}
}
