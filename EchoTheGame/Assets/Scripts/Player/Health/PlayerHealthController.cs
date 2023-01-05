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

	public override void Spawned()
	{
		base.Spawned();

		PlayerHealth = _maxHealth;

		_playerNetworkController = GetComponentInChildren<PlayerNetworkedController>();
		_healthBarSlider = Instantiate(_healthBarPrefab).GetComponentInChildren<HealthBarPositionBehaviour>();
		_healthBarSlider.Init(transform, _maxHealth);
	}

	public void Respawn()
	{
		_healthBarSlider.gameObject.SetActive(true);
		PlayerHealth = _maxHealth;
		_healthBarSlider.UpdateSlider(PlayerHealth);
	}

	public void HitPlayer(int damage)
	{
		PlayerHealth -= damage;
	}

	private void UpdateHealth()
{
		_healthBarSlider.UpdateSlider(PlayerHealth);

		if (PlayerHealth < 0)
		{
			Alive = false;
			_healthBarSlider.gameObject.SetActive(false);
			_playerNetworkController.DisablePlayer();
			_playerNetworkController.RespawnPlayer(2.5f);
		}
	}

	public static void UpdateHealthBar(Changed<PlayerHealthController> changed)
	{
		var thisObject = changed.Behaviour;
		thisObject.UpdateHealth();
	}

	private static void OnAliveChanged(Changed<PlayerHealthController> changed)
	{
		
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
		base.Despawned(runner, hasState);
	}
}
