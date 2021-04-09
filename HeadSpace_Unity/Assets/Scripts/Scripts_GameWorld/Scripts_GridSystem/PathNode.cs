using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridTile[,] grid;
    public GridTile tile;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isTraversable;
    public PathNode cameFromNode;

    public PathNode(GridTile[,] grid, GridTile tile, int x, int y)
    {
        this.grid = grid;
        this.tile = tile;
        this.x = x;
        this.y = y;
        isTraversable = tile.tileType == 1? false : true;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return x + "," + y;
    }
}
