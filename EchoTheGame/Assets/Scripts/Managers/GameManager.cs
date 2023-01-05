using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager _instance;

	private byte[] _connectionToken;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		_connectionToken = ConnectionTokenUtils.NewToken();
	}

	public static void SetConnectionToken(byte[] connectionToken)
	{
		_instance._connectionToken = connectionToken;
	}

	public static byte[] GetConnectionToken()
	{
		return _instance._connectionToken;
	}
}
