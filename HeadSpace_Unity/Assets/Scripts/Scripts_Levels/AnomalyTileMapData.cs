using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AnomalyTileMapData : MonoBehaviour
{
    private void Start()
    {
        //GetTileData();
    }

    public int[,] GetTileData()
    {
        Tilemap tilemap = GetComponentInChildren<Tilemap>();
        tilemap.CompressBounds();
        tilemap.origin = tilemap.cellBounds.min;

        Vector2Int tilemapSize = (Vector2Int)tilemap.size;
        int[,] tileData = new int[tilemapSize.x, tilemapSize.y];

        Vector3Int tilePosition = new Vector3Int();
        for (int y = tilemapSize.y - 1; y >= 0; y--)
        {
            tilePosition.y = y + tilemap.origin.y;
            for (int x = 0; x < tilemapSize.x; x++)
            {
                tilePosition.x = x + tilemap.origin.x;
                if (tilemap.GetTile(tilePosition).name == "TileMapTool_RockTile")
                {
                    tileData[x, tilemapSize.y - 1 - y] = 1;
                }
                else if (tilemap.GetTile(tilePosition).name == "TileMapTool_StaticAnomalyTile")
                {
                    tileData[x, tilemapSize.y - 1 - y] = 4;
                }
                else
                {
                    tileData[x, tilemapSize.y - 1 - y] = 0;
                }
            }
        }

        return tileData;
    }
}
