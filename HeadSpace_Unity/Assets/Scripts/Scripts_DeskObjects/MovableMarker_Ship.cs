﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMarker_Ship : MovableMarker
{
    private Canvas _callSignCanvas;

    protected override void Awake()
    {
        base.Awake();
        _callSignCanvas = GetComponentInChildren<Canvas>();
    }

    public override void Select(bool fireEvent = true)
    {
        base.Select(fireEvent);

        if (_callSignCanvas != null)
        {
            _callSignCanvas.sortingLayerID = _spriteRenderer.sortingLayerID;
            _callSignCanvas.sortingOrder = _spriteRenderer.sortingOrder;
        }
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);

        if (_callSignCanvas != null)
        {
            _callSignCanvas.sortingLayerID = _spriteRenderer.sortingLayerID;
            _callSignCanvas.sortingOrder = _spriteRenderer.sortingOrder;
        }
    }
}
