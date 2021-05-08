using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyPatch
{
    private List<GridTile> _allTiles;
    private List<GridTile> _candidateTiles = new List<GridTile>();
    private int _candidateCount;

    public AnomalyPatch(List<GridTile> gridTiles)
    {
        _allTiles = gridTiles;
        AssignCandidateTiles();
    }

    private void OnTileDestroyed(GridTile_StaticAnomaly anomaly)
    {
        anomaly.anomalyTileDestroyed -= OnTileDestroyed;
        _allTiles.Remove(anomaly);
        AssignCandidateTiles();
    }

    private void AssignCandidateTiles()
    {
        _candidateTiles.Clear();
        _candidateCount = 0;

        foreach (var tile in _allTiles)
        {
            foreach (var neighbour in tile.AllNeighbours)
            {
                if (neighbour != null)
                    if (neighbour.tileType == 0)
                    {
                        _candidateTiles.Add(neighbour);
                        _candidateCount++;
                    }
            }
        }
    }

    public GridTile GetRandomNewTile()
    {
        AssignCandidateTiles();

        if (_candidateCount <= 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, _candidateCount);
        GridTile candidate = _candidateTiles[randomIndex];
        _candidateTiles.Remove(candidate);
        _allTiles.Remove(candidate);
        _candidateCount--;

        return candidate;
    }

    public void AddTileToPatch(GridTile newTile)
    {
        _allTiles.Add(newTile);
        GridTile_StaticAnomaly anomaly = newTile.GetComponent<GridTile_StaticAnomaly>();

        anomaly.anomalyTileDestroyed += OnTileDestroyed;
    }
}
