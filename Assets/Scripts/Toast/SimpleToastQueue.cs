using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleToastQueue : IToastQueue
{
    private readonly int _maxVisible;
    private int _visible = 0;

    public SimpleToastQueue(int maxVisible = 2) { _maxVisible = Mathf.Max(1, maxVisible); }
    public bool HasCapacity => _visible < _maxVisible;
    public void OnToastStarted() { _visible++; }
    public void OnToastFinished() { _visible = Mathf.Max(0, _visible - 1); }
}
