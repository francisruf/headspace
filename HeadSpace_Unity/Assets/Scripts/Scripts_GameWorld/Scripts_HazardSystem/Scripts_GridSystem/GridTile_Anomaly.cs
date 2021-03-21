using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_Anomaly : GridTile
{
    public static Action<GridTile_Anomaly> newAnomalyTile;

    public string tileNameLine;
    public SpriteRenderer anomalySpriteRenderer;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (newAnomalyTile != null)
            newAnomalyTile(this);
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        base.InitializeTile(tileDimensions, gridMode);

        if (anomalySpriteRenderer != null)
        {
            anomalySpriteRenderer.size = tileDimensions;
        }
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        base.InitializeTile(tileDimensions, gridMode, currentGridInfo);

        if (anomalySpriteRenderer != null)
        {
            anomalySpriteRenderer.size = tileDimensions;
        }
    }

    protected override void ToggleGridMode(GridMode newGridMode)
    {
        base.ToggleGridMode(newGridMode);

        if (anomalySpriteRenderer == null)
            return;

        switch (newGridMode)
        {
            case GridMode.WorldMap:
                anomalySpriteRenderer.enabled = true;
                break;
            case GridMode.Debug:
                anomalySpriteRenderer.enabled = false;
                break;
            default:
                break;
        }
    }
}
