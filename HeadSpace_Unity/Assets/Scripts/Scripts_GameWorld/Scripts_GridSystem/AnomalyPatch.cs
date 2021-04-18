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
        Debug.Log("NEW PATCH WITH " + _allTiles.Count + " TILES");
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
        Debug.Log("CANDIDATE COUNT : " + _candidateCount);
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
    }
}
