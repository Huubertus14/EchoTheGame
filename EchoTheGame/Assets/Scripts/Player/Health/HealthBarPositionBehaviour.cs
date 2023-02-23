using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;

public class HealthBarPositionBehaviour : MonoBehaviour
{
	public byte[] connectionToken { get; private set; }
	private Transform _followTarget;
	private Slider _healthSlider;
	[SerializeField] private Vector3 _positionOffset;
	private Quaternion _originalWorldRotation;

	public void Init(byte[] connectionToken,Transform target, int maxHealth)
	{
		_followTarget = target;
		_healthSlider = GetComponentInChildren<Slider>();
		_healthSlider.maxValue = maxHealth;
		_healthSlider.value = maxHealth;
		_originalWorldRotation = transform.rotation;
	}

	public void UpdateSlider(int health)
	{
		_healthSlider.value = health;
	}

	internal void SetPosition()
	{
		transform.rotation = _originalWorldRotation;
		transform.position = _followTarget.position + _positionOffset;
	}
}
