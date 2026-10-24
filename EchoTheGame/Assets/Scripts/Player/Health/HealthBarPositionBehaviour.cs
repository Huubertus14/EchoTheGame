using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;
using Project.Echo.Menu;

public class HealthBarPositionBehaviour : MonoBehaviour
{
	public byte[] connectionToken { get; private set; }
	private Transform _followTarget;
	private Slider _healthSlider;
	[SerializeField] private Vector3 _positionOffset;
	[SerializeField] private EchoMenuTheme _theme;
	[SerializeField] private Image _fill;

	public void Init(byte[] connectionToken,Transform target, int maxHealth)
	{
		this.connectionToken = connectionToken;
		_followTarget = target;
		_healthSlider = GetComponentInChildren<Slider>();
		_healthSlider.maxValue = maxHealth;
		_healthSlider.value = maxHealth;
		UpdateSlider(maxHealth);
	}

	public void UpdateSlider(int health)
	{
		if (!_healthSlider) return;
		_healthSlider.value = Mathf.Clamp(health, 0, _healthSlider.maxValue);
		if (_fill && _theme)
			_fill.color = _healthSlider.normalizedValue <= 0.25f ? _theme.amber : _theme.seafoam;
	}

	internal void SetPosition()
	{
		if (_followTarget) transform.position = _followTarget.position + _positionOffset;
	}

	private void LateUpdate() => SetPosition();
}
