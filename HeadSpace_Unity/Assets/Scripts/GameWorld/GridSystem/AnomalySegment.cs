using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalySegment
{
    public static Action<GridTile, int> newTileRequest;

    public GridTile activeTile;
    private List<GridTile> _allSegmentTiles = new List<GridTile>();

    // TODO : Improve performance of that stuff
    private List<GridTile> _allCandidates = new List<GridTile>();

    public void AssignActiveTile(GridTile newActiveTile)
    {
        activeTile = newActiveTile;
        activeTile.tileLifeOver += OnTileLifeOver;
    }

    private bool SelectNextAnomalyTile(out TileCoordinates nextTileCoordinates)
    {
        // Check scenarios here
        // Wrap here into default scenario
        CalculatePotentialCandidates();

        nextTileCoordinates = default(TileCoordinates);

        if (_allCandidates.Count > 0)
        {
            int randomID = UnityEngine.Random.Range(0, _allCandidates.Count);
            nextTileCoordinates = new TileCoordinates(_allCandidates[randomID].tileX, _allCandidates[randomID].tileY);
            return true;
        }
        return false;
    }

    private void CalculatePotentialCandidates()
    {
        _allCandidates.Clear();
        foreach (var tile in _allSegmentTiles)
        {
            foreach (var emptyNeighbour in tile.EmptyNeighbours)
            {
                _allCandidates.Add(emptyNeighbour);
            }
        }
    }

    // Fonction qui gère le passage d'une tuile à une autre, quand son temps de vie est écoulé.
    private void OnTileLifeOver(GridTile deadTile)
    {
        // Unsubscribe à l'event de la tuile active
        if (deadTile == activeTile)
            activeTile.tileLifeOver -= OnTileLifeOver;

        RemoveTileFromSegment(deadTile);
        GridTile newActiveTile;

        switch (deadTile.tileType)
        {
            case 1:
                {
                    GridManager.instance.ReplaceTile(deadTile, 2, out newActiveTile);
                    break;
                }

            case 2:
                {
                    GridTile newFinishedTile;
                    GridManager.instance.ReplaceTile(deadTile, 3, out newFinishedTile);
                    AddTileToSegment(newFinishedTile);

                    TileCoordinates nextTileCoords;
                    bool gridHasEmptyTile = SelectNextAnomalyTile(out nextTileCoords);

                    if (gridHasEmptyTile)
                    {
                        GridManager.instance.ReplaceTile(nextTileCoords, 1, out newActiveTile);
                    }
                    else
                    {
                        Debug.Log("ENDEUH GAMEUH");
                        return;
                        // END GAME
                    }
                    break;
                }

            default:
                {
                    newActiveTile = deadTile;
                    break;
                }
        }
        AssignActiveTile(newActiveTile);
        AddTileToSegment(newActiveTile);
    }

    private void AddTileToSegment(GridTile tile)
    {
        _allSegmentTiles.Add(tile);
        Debug.Log("Tile count : " + _allSegmentTiles.Count);
    }

    private void RemoveTileFromSegment(GridTile tile)
    {
        if (_allSegmentTiles.Contains(tile))
            _allSegmentTiles.Remove(tile);
    }
}
