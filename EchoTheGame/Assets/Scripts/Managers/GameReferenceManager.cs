using Project.Echo.Caches;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameReferenceManager : MonoBehaviour
{
	private static GameReferenceManager _instance;

    public static ObjectCache GetObjectCache => _instance._objectCache;
   [SerializeField] private ObjectCache _objectCache;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}

		_objectCache.Initialize();
	}

	private void OnDestroy()
	{
		_objectCache.Deinitialize();
	}
}
