using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableScissors : MovableObject
{
    public static Action scissorsCut;
    private bool _isOpen;

    [Header("Scissors settings")]
    public Sprite openSprite;
    public Sprite closedSprite;
    public Transform circleCastPos;

    public LayerMask tileLayerMask;

    protected override void Update()
    {
        base.Update();
        if (IsSelected && Input.GetMouseButtonDown(1))
        {
            CloseScissors();
        }

        if (!_isOpen && !Input.GetMouseButton(1))
        {
            OpenScissors();
        }
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);
        OpenScissors();
    }

    private void CloseScissors()
    {
        _isOpen = false;
        _spriteRenderer.sprite = closedSprite;
        _shadowRenderer.sprite = closedSprite;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(circleCastPos.position, 0.13f, tileLayerMask);
        Vector2 selectedPos = new Vector2(100f, -100f);
        GridTile_StaticAnomaly candidate = null;

        foreach (var col in colliders)
        {
            GridTile_StaticAnomaly anomaly = col.GetComponent<GridTile_StaticAnomaly>();
            if (anomaly != null)
            {
                if (anomaly.transform.position.y > selectedPos.y || anomaly.transform.position.x > selectedPos.x)
                {
                    candidate = anomaly;
                    selectedPos = anomaly.transform.position;
                }
            }
        }

        if (candidate != null)
            candidate.Hit(circleCastPos.position);

        if (scissorsCut != null)
            scissorsCut();
    }

    private void OpenScissors()
    {
        _isOpen = true;
        _spriteRenderer.sprite = openSprite;
        _shadowRenderer.sprite = openSprite;
    }

}
