using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;
using Project.Echo.Menu;
using Project.Echo.Tuning;

public class HealthBarPositionBehaviour : MonoBehaviour
{
	public byte[] connectionToken { get; private set; }
	private Transform _followTarget;
	private Slider _healthSlider;
	private SubmarineSettings _settings;
	[SerializeField] private EchoMenuTheme _theme;
	[SerializeField] private Image _fill;

	public void Init(byte[] connectionToken, Transform target, SubmarineSettings settings)
	{
		this.connectionToken = connectionToken;
		_followTarget = target;
		_settings = settings;
		_healthSlider = GetComponentInChildren<Slider>();
		_healthSlider.maxValue = settings.maxHealth;
		UpdateSlider(settings.maxHealth);
	}

	public void UpdateSlider(int health)
	{
		if (!_healthSlider) return;
		_healthSlider.value = Mathf.Clamp(health, 0, _healthSlider.maxValue);
		if (_fill && _theme)
			_fill.color = _healthSlider.normalizedValue <= _settings.lowHealthThreshold ? _theme.amber : _theme.seafoam;
	}

	internal void SetPosition()
	{
		if (_followTarget) transform.position = _followTarget.position + _settings.healthBarOffset;
	}

	private void LateUpdate() => SetPosition();
}
