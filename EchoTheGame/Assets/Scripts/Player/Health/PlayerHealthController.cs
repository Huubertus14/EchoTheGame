using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Project.Echo.Projectiles.Enums;
using Project.Echo.Player.Visuals;
using Project.Echo.Player;
using System;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField]private GameObject _healthBarPrefab;

    [Networked(OnChanged = nameof(UpdateHealthBar))]
    public int PlayerHealth { get; set; }

	private int _maxHealth = 100;
	private HealthBarPositionBehaviour _healthBarSlider;
	private PlayerNetworkedController _playerNetworkController;

	public override void Spawned()
	{
		base.Spawned();

		PlayerHealth = _maxHealth;

		_playerNetworkController = GetComponentInChildren<PlayerNetworkedController>();
		_playerNetworkController.OnRespawned += Respawned;
		_healthBarSlider = Instantiate(_healthBarPrefab).GetComponentInChildren<HealthBarPositionBehaviour>();
		_healthBarSlider.Init(transform, _maxHealth);
	}

	private void Respawned()
	{
		_healthBarSlider.gameObject.SetActive(true);
		PlayerHealth = _maxHealth;
		_healthBarSlider.UpdateSlider(PlayerHealth);
	}

	public void HitPlayer(int damage)
	{
		PlayerHealth -= damage;
	}

	public static void UpdateHealthBar(Changed<PlayerHealthController> changed)
	{
		var thisObject = changed.Behaviour;

		thisObject._healthBarSlider.UpdateSlider(changed.Behaviour.PlayerHealth);

		if (thisObject.PlayerHealth < 0)
		{
			thisObject._healthBarSlider.gameObject.SetActive(false);
			thisObject._playerNetworkController.DisablePlayer();
			thisObject._playerNetworkController.RespawnPlayer(2.5f);
		}

	}

	public override void FixedUpdateNetwork()
	{
		base.FixedUpdateNetwork();
		_healthBarSlider.SetPosition();
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		_playerNetworkController.OnRespawned -= Respawned;
		Destroy(_healthBarSlider.gameObject);
		base.Despawned(runner, hasState);
	}
}
