using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(MeshRenderer))]
public class SonarObject : MonoBehaviour, IHitSonarAble
{
	MeshRenderer _meshRenderer;

	private Color _hitColor;
	private Color _offColor;

	private bool ishit;

	private IEnumerator _fadeEffect;

	float durationTo = 120;
	float durationFrom = 60;

	private void Awake()
	{
		_meshRenderer = GetComponent<MeshRenderer>();
		_hitColor = Color.blue;
		_offColor = Color.black; 
		_meshRenderer.material.color = _offColor;
	}

	public void HitBySonar(Vector3 firstHitPosition)
	{
		if (!ishit)
		{
			//Do fancy transition thing
			ObjectHitEffect(Color.blue, firstHitPosition);
			_fadeEffect ??= FadeOut();
			StartCoroutine(_fadeEffect);
		}
	}

	private void ObjectHitEffect(Color playerColor, Vector3 firstParticlePosition)
	{
		_meshRenderer.material.SetVector("_SonarWaveVector", firstParticlePosition);
		_meshRenderer.material.SetColor("_SonarWaveColor", playerColor);
	}

	

	private IEnumerator FadeOut()
	{
		ishit = true;
		_meshRenderer.material.EnableKeyword("VISIBLE");

		for (int i = 0; i < durationTo; i++)
		{
			_meshRenderer.material.SetFloat("_SonarStep", i/durationTo);
			yield return new WaitForSeconds(1f/50);
		}
		yield return 0;
		_meshRenderer.material.DisableKeyword("VISIBLE");
		for (int i = 0; i < durationFrom; i++)
		{
			_meshRenderer.material.SetFloat("_SonarStep", i / durationFrom);
			yield return new WaitForSeconds(1f / 50);
		}
		ishit = false;
		_fadeEffect = null;
	}
}
