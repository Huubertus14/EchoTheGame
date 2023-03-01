using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchController : MonoBehaviour
{
	private Camera _mainCamera;

	EventTrigger _trigger;

	private void Awake()
	{
		_mainCamera = Camera.main;
		
	}

	private void Update()
	{
		foreach (var touch in Input.touches)
		{
			var ray = _mainCamera.ScreenPointToRay(touch.position);
			
		}
	}

	
}
