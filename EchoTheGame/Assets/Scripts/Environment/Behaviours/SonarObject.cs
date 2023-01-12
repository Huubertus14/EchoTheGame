using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SonarObject : MonoBehaviour, IHitSonarAble
{
	MeshRenderer _meshRenderer;

	private Color _hitColor;
	private Color _offColor;

	private float _lerpedValue;
	private Color _lerpedColor;

	private IEnumerator _faceCoroutine;

	private bool ishit;

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_hitColor = Color.blue;
		_offColor = Color.black;
	}

	public void HitBySonar(Vector3 firstHitPosition)
	{
		if (!ishit)
		{
			ishit = true;

			//Do fancy transition thing
		}
		else
		{
			//Update color and reset lerped reset thing
		}
	}

	private void OnParticleCollision(GameObject other)
	{
		//Debug.Log($"Hit {other.name}", other);
		return; //TODO add logic not in this method
		if(_faceCoroutine == null)
		{
			_faceCoroutine = ObjectHitEffect();
			StartCoroutine(ObjectHitEffect());
		}
		else
		{
			_lerpedValue = 1;
		}
	}

	private IEnumerator ObjectHitEffect()
	{
		_meshRenderer.material.color = _hitColor;
		_lerpedColor = _hitColor;
		float duration = 1.5f;
		_lerpedValue = 1;

		//todo lerp the fade in to whole object

		//if fadein is done fade out  u8*

		while(_lerpedValue >= 0)
		{
			_lerpedValue -= Time.deltaTime / duration;
			_lerpedColor = Color.Lerp(_offColor, _hitColor, _lerpedValue);
			_meshRenderer.material.color = _lerpedColor;
			yield return 0;
		}
		_meshRenderer.material.color = _offColor;
		_faceCoroutine = null;
	}
}
