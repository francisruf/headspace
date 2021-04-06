using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_DeployPoint : GridTile
{
    public static Action<GridTile_DeployPoint> newDeployTile;

    private void OnEnable()
    {
        if (newDeployTile != null)
            newDeployTile(this);
    }
}
