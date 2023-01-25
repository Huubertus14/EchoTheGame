using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LoadingScreenRotatingIcon : MonoBehaviour
{
    private Task _rotatingTask;

	private void OnEnable()
	{
		_rotatingTask ??= Rotate();
	}

	private async Task Rotate()
	{
		while (enabled)
		{
			transform.Rotate(new Vector3(0,0,-50)*Time.deltaTime);
			await UniTask.NextFrame();
		}
	}

	private void OnDisable()
	{
		_rotatingTask = null;
	}
}
