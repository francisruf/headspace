using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMarker : MovableObject
{
    public static Action<MovableMarker> markerPinnedOnTile;

    protected TileCoordinates _currentTile = new TileCoordinates();
    protected bool _isInTile;

    protected override void Start()
    {
        base.Start();
        CheckIfInTile();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Select()
    {
        base.Select();
        _mouseOffset = new Vector2(0.04f, 0f);

        //Debug.Log(GridCoords.FromWorldToGrid(transform.position));
    }

    public override void Deselect()
    {
        base.Deselect();
        if (_currentDropZone == null)
            CheckIfInTile();
    }

    protected virtual void CheckIfInTile()
    {
        _isInTile = GridCoords.IsInTile(transform.position, out _currentTile);

        if (_isInTile)
        {
            if (markerPinnedOnTile != null)
                markerPinnedOnTile(this);
        }
    }
}
