using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMarker : MovableObject
{
    public static Action<MovableMarker> markerPinnedOnTile;

    protected TileCoordinates _currentTile = new TileCoordinates();
    protected bool _isInTile;

    [Header("Marker sprites")]
    public Sprite upSprite;
    public Sprite angleSprite;

    protected override void Awake()
    {
        base.Awake();
        if (upSprite == null)
            upSprite = _spriteRenderer.sprite;
        if (angleSprite == null)
            angleSprite = _spriteRenderer.sprite;
    }

    protected override void Start()
    {
        base.Start();
        CheckIfInTile();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Select(bool fireEvent = true)
    {
        _spriteRenderer.sprite = upSprite;

        base.Select(fireEvent);
        _mouseOffset = new Vector2(0.04f, 0f);

        //Debug.Log(GridCoords.FromWorldToGrid(transform.position));
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);
        CheckIfInTile();
    }

    protected virtual void CheckIfInTile()
    {
        if (_currentDropZone != null)
        {
            _spriteRenderer.sprite = angleSprite;
            return;
        }
            

        _isInTile = GridCoords.IsInTile(transform.position, out _currentTile);

        if (_isInTile)
        {
            _spriteRenderer.sprite = upSprite;

            if (markerPinnedOnTile != null)
                markerPinnedOnTile(this);

            int sortingOrder = (int)(5000f - (transform.position.y * 32));
            bool overlap = ObjectsManager.instance.ObjectOverlap(this);
            if (overlap)
            {
                sortingOrder += 1;
            }

            SetOrderInLayer(sortingOrder, false);
        }
        else
        {
            _spriteRenderer.sprite = angleSprite;
        }
    }
}
