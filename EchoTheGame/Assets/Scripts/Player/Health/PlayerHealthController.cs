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

	private int _maxHealth = 100;
	private HealthBarPositionBehaviour _healthBarSlider;
	private PlayerNetworkedController _playerNetworkController;
	[SerializeField]private PlayerScoreboardController _playerScoreBoard;
	public bool SkipInit = false;
	private string _lastHitByPlayer;

	public override void Spawned()
	{
		base.Spawned();

		if (!SkipInit)
		{
			PlayerHealth = _maxHealth;
		}

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

	private void OnRespawned()
	{
		PlayerHealth = _maxHealth;
		_healthBarSlider.gameObject.SetActive(true);
		_healthBarSlider.UpdateSlider(PlayerHealth);
	}

	public void HitPlayer(int damage, string hitByPlayer)
	{
		PlayerHealth -= damage;
		_lastHitByPlayer = hitByPlayer;
	}

	private void UpdateHealth()
{
		_healthBarSlider.UpdateSlider(PlayerHealth);

		if (PlayerHealth < 0)
		{
			Alive = false;
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

		if(newValue)
		{
			changed.Behaviour.OnRespawned();
		}
		else 
		{
			changed.Behaviour.PlayerDied();
		}
	}

	private void PlayerDied()
	{
		if(HasStateAuthority)
		{
			RPC_SendKillMessageToServer(Runner,_playerScoreBoard.GetPlayerName, _lastHitByPlayer);
		}
		_healthBarSlider.gameObject.SetActive(false);
		_playerNetworkController.DisablePlayer();
		_playerNetworkController.RespawnPlayer(2.5f);
	}

	
	private static void RPC_SendKillMessageToServer(NetworkRunner runner,string killer, string victim)
	{
		if(PlayerNetworkedController.LocalPlayer.HasInputAuthority)
		{
			KillFeedController.SetKillFeed($"<b>{killer}</b> killed <b>{victim}</b>");
		}
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
			Destroy(_healthBarSlider.gameObject);
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
