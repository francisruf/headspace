using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_Anomaly : GridTile
{
    public static Action<GridTile_Anomaly> newAnomalyTile;

    protected override void Start()
    {
        base.Start();

        if (newAnomalyTile != null)
            newAnomalyTile(this);
    }
}
