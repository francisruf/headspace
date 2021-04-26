using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_StaticAnomaly : GridTile
{
    public SpriteRenderer _tentaclesRenderer;

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        base.InitializeTile(tileDimensions, gridMode);
        SetTentaclePosition();
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        base.InitializeTile(tileDimensions, gridMode, currentGridInfo);
        SetTentaclePosition();
    }

    private void SetTentaclePosition()
    {
        Vector2 pos = _spriteRenderer.bounds.min;
        pos.x = _spriteRenderer.bounds.max.x;
        //Debug.DrawLine(_spriteRenderer.bounds.min, _spriteRenderer.bounds.min + Vector3.up * 0.5f, Color.yellow);
        _tentaclesRenderer.transform.position = pos;
    }
}
