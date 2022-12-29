using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;

public class HealthBarPositionBehaviour : MonoBehaviour
{
	private Transform _followTarget;
	private Slider _healthSlider;
	[SerializeField] private Vector3 _positionOffset;

	public void Init(Transform target, int maxHealth)
	{
		_followTarget = target;
		_healthSlider = GetComponentInChildren<Slider>();
		_healthSlider.maxValue = maxHealth;
		_healthSlider.value = maxHealth;
	}

	public void UpdateSlider(int health)
	{
		_healthSlider.value = health;
	}

	internal void SetPosition()
	{
		transform.position = _followTarget.position + _positionOffset;
	}
}
