using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardManager : MonoBehaviour
{
    // Singleton
    public static HazardManager instance;

    [Header("Hazard settings")]
    public int electricCloudCount;
    public int wormHolePairCount;

    [Header("TODO - Spawning settings")]
    public int minWormholeDistanceInTiles;

    [Header("Prefabs")]
    // Prefab
    public GameObject cloudPrefab;
    public GameObject wormHolePrefab;

    // Informations de la grille de jeu actuelle
    private GridInfo _currentGridInfo;

    // Tous les hazards instanciés
    private List<Hazard> _allHazards = new List<Hazard>();

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    // Subscription à différentes actions
    private void OnEnable()
    {
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
        GridManager.newGameGrid += OnNewGameGrid;
        //GridManager.firstAnomalyTile += OnFirstAnomalyTile;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
        GridManager.newGameGrid -= OnNewGameGrid;
        //GridManager.firstAnomalyTile -= OnFirstAnomalyTile;
    }

    // Fonction appelée lorsqu'une nouvelle grille est générée qui stock les informations de cette grille
    private void OnNewGameGrid(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
    }

    // Fonction appelée lorsque l'anomalie a été assignée, afin d'assigner par la suite les planètes
    //private void OnFirstAnomalyTile()
    //{
    //    SpawnHazards();
    //}

    // Fonction appelée lorsque la grille actuelle est détruite qui reset les variables et références
    private void OnGridDataDestroyed()
    {
        _allHazards.Clear();
        _currentGridInfo = null;
    }

    public void SpawnHazards()
    {
    //    List<GridTile> allowedSpawnTiles = GetAllowedSpawnTiles();

    //    for (int i = 0; i < electricCloudCount; i++)
    //    {
    //        if (allowedSpawnTiles.Count <= 0)
    //        {
    //            Debug.Log("No more tiles available for hazard spawning!");
    //            break;
    //        }

    //        int randomIndex = UnityEngine.Random.Range(0, allowedSpawnTiles.Count);
    //        GridTile randomTile = allowedSpawnTiles[randomIndex];
    //        Vector2 spawnPos = GridCoords.GetRandomCoordsInTile(randomTile);
    //        Cloud cloud = Instantiate(cloudPrefab).GetComponent<Cloud>();
    //        cloud.PlaceGridObject(spawnPos);
    //        _allHazards.Add(cloud);

    //        foreach (var nb in randomTile.EightWayNeighbours)
    //        {
    //            allowedSpawnTiles.Remove(nb);
    //        }

    //        allowedSpawnTiles.Remove(randomTile);
    //    }

    //    List<GridTile> tempRemovedTiles = new List<GridTile>();

    //    for (int i = 0; i < wormHolePairCount; i++)
    //    {
    //        if (allowedSpawnTiles.Count <= 0)
    //        {
    //            Debug.Log("No more tiles available for hazard spawning!");
    //            break;
    //        }

    //        int randomIndex = UnityEngine.Random.Range(0, allowedSpawnTiles.Count);
    //        GridTile randomTile = allowedSpawnTiles[randomIndex];
    //        Vector2 spawnPos = GridCoords.GetRandomCoordsInTile(randomTile);
    //        WormHole wh1 = Instantiate(wormHolePrefab).GetComponent<WormHole>();
    //        wh1.PlaceGridObject(spawnPos);
    //        _allHazards.Add(wh1);

    //        allowedSpawnTiles.Remove(randomTile);

    //        foreach (var nb in randomTile.EightWayNeighbours)
    //        {
    //            foreach (var nbnb in nb.EightWayNeighbours)
    //            {
    //                allowedSpawnTiles.Remove(nbnb);
    //                tempRemovedTiles.Remove(nbnb);
    //            }
    //            allowedSpawnTiles.Remove(nb);
    //        }

    //        if (allowedSpawnTiles.Count <= 0)
    //        {
    //            Debug.Log("No more tiles available for hazard spawning!");
    //            _allHazards.Remove(wh1);
    //            Destroy(wh1.gameObject);
    //            break;
    //        }

    //        randomIndex = UnityEngine.Random.Range(0, allowedSpawnTiles.Count);
    //        GridTile randomTile2 = allowedSpawnTiles[randomIndex];
    //        spawnPos = GridCoords.GetRandomCoordsInTile(randomTile2);
    //        WormHole wh2 = Instantiate(wormHolePrefab).GetComponent<WormHole>();
    //        wh2.PlaceGridObject(spawnPos);
    //        _allHazards.Add(wh2);

    //        allowedSpawnTiles.Remove(randomTile2);

    //        foreach (var tile in tempRemovedTiles)
    //        {
    //            allowedSpawnTiles.Add(tile);
    //        }

    //        foreach (var nb in randomTile2.EightWayNeighbours)
    //        {
    //            allowedSpawnTiles.Remove(nb);
    //        }

    //        wh1.SisterWormHole = wh2;
    //        wh2.SisterWormHole = wh1;
    //    }
    }

    private List<GridTile> GetAllowedSpawnTiles()
    {
        List<GridTile> candidateTiles = _currentGridInfo.GetEmptyTiles();
        List<GridTile> anomalyTiles = _currentGridInfo.GetAnomalyTiles();
        List<GridTile> neighbours1 = new List<GridTile>();
        List<GridTile> neighbours2 = new List<GridTile>();
        List<GridTile> allNeighbours = new List<GridTile>();

        foreach (var tile in anomalyTiles)
        {
            foreach (var emptyNeighbour in tile.EmptyNeighbours)
            {
                neighbours1.Add(emptyNeighbour);
                allNeighbours.Add(emptyNeighbour);
            }
        }

        foreach (var tile in neighbours1)
        {
            foreach (var emptyNeighbour in tile.EmptyNeighbours)
            {
                neighbours2.Add(emptyNeighbour);
                allNeighbours.Add(emptyNeighbour);
            }
        }

        foreach (var tile in allNeighbours)
        {
            if (candidateTiles.Contains(tile))
            {
                candidateTiles.Remove(tile);
            }
        }

        if (DeployManager.instance != null)
        {
            foreach (var tile in DeployManager.instance.GetAllDeployTouchingTiles())
            {
                if (candidateTiles.Contains(tile))
                {
                    candidateTiles.Remove(tile);
                }
            }
        }
        else
        {
            Debug.LogError("WARNING : Could not find DeployManager");
        }

        return candidateTiles;
    }

    public void ToggleHazardDebug(bool toggleON)
    {
        foreach (var hazard in _allHazards)
        {
            hazard.ToggleSprite(toggleON);
        }
    }
}
