using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(NetworkObject))]
public class NetworkObjectDespawner : MonoBehaviour
{
	[SerializeField]private float _lifeTime;

	private NetworkObject _networkObject;

	private void Awake()
	{
		_networkObject = GetComponent<NetworkObject>();
	}

	private void FixedUpdate()
	{
		_lifeTime -= _networkObject.Runner.DeltaTime;
		if (_lifeTime <0)
		{
			_networkObject.Runner.Despawn(_networkObject);
		}
	}
}
