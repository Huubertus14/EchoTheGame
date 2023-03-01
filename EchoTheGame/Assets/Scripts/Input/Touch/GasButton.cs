using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GasButton : BaseTouchButton 
{
	private RectTransform _rectTransform;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
	}

	protected override void PointerEnter(PointerEventData eventData)
	{
		_rectTransform.localScale = Vector3.one * 4; 
		//ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.pointerEnterHandler);

	}

	public override void PointerExit(PointerEventData eventData)
	{
		//ExecuteEvents.ExecuteHierarchy(gameObject, eventData, ExecuteEvents.pointerExitHandler);
		_rectTransform.localScale = Vector3.one;
	}
}
