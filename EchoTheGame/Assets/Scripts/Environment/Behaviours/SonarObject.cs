using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(MeshRenderer))]
public class SonarObject : MonoBehaviour, IHitSonarAble
{
	private const string _visibleKeyword = "VISIBLE";

	private MeshRenderer _meshRenderer;
	private IEnumerator _fadeEffect;

	private bool _ishit;
	private int _sonarStepId;
	private int _sonarWaveVectorId;
	private int _sonarWaveColorId;

	private float _durationToInSeconds = 4f;
	private float _durationFromInSeconds = 1.5f;

	private void Awake()
	{
		_sonarStepId = Shader.PropertyToID("_SonarStep");
		_sonarWaveVectorId = Shader.PropertyToID("_SonarWaveVector");
		_sonarWaveColorId = Shader.PropertyToID("_SonarWaveColor");

		_meshRenderer = GetComponent<MeshRenderer>();
		_meshRenderer.material.SetColor("_BaseColor", Color.black);// = Color.black; 
		_meshRenderer.material.SetFloat(_sonarStepId, 1);
	}

	public void HitBySonar(Vector3 firstHitPosition, Color hitColor)
	{
		if (!_ishit)
		{
			//Do fancy transition thing
			ObjectHitEffect(hitColor, firstHitPosition);
			_fadeEffect ??= FadeOut();
			StartCoroutine(_fadeEffect);
		}
	}

	private void ObjectHitEffect(Color playerColor, Vector3 firstParticlePosition)
	{
		_meshRenderer.material.SetVector(_sonarWaveVectorId, firstParticlePosition);
		_meshRenderer.material.SetColor(_sonarWaveColorId, playerColor);
	}

	private IEnumerator FadeOut() //TODO change this to use normal logic
	{
		_ishit = true;
		_meshRenderer.material.EnableKeyword(_visibleKeyword);

		float indexer = 0;

		while (indexer < 1)
		{
			indexer += Time.deltaTime / _durationToInSeconds; 
			_meshRenderer.material.SetFloat(_sonarStepId, indexer);
			yield return 0;
		}

		_meshRenderer.material.DisableKeyword(_visibleKeyword);

		indexer = 0;
		while (indexer < 1)
		{
			indexer += Time.deltaTime / _durationFromInSeconds;
			_meshRenderer.material.SetFloat(_sonarStepId, indexer);
			yield return 0;
		}

		_meshRenderer.material.SetFloat(_sonarStepId, 1);
		_ishit = false;
		_fadeEffect = null;
	}
}
