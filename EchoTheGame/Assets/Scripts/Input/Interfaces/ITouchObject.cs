using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITouchObject
{
    public void OnTouchBegin();
    public void OnTouchHold();
    public void OnTouchReleased();
}
