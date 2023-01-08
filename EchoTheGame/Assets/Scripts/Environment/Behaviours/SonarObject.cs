using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SonarObject : MonoBehaviour
{
	MeshRenderer _meshRenderer;

	private Color _hitColor;
	private Color _offColor;
	private Color _objectColor;

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_hitColor = Color.blue;
		_offColor = Color.black;
	}

	private void OnParticleCollision(GameObject other)
	{
		Debug.Log($"Hit {other.name}", other);
		StartCoroutine(ObjectHitEffect());
	}

	private IEnumerator ObjectHitEffect()
	{
		var q = _hitColor;
		_meshRenderer.material.color = _hitColor;

		float duration = 1.5f;
		var startTime = Time.time;

		yield return new WaitForSeconds(1);
		_meshRenderer.material.color = _offColor;
		

		yield return 0;
	}
}
