using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseTouchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public virtual bool IsPressed { get; set; }

	public void OnPointerEnter(PointerEventData eventData)
	{
		IsPressed = true;
		PointerEnter(eventData);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsPressed = false;
		PointerExit(eventData);
	}

	protected virtual void PointerEnter(PointerEventData eventData) { }
	public virtual void PointerExit(PointerEventData eventData) { }
}
