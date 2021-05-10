using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    // Singleton
    public static PlanetManager instance;

    // Action qui envoie toutes les planètes spawnées au début d'une partie
    public static Action<List<Planet>> planetsSpawned;
    public static Action noMoreSoulsInSector;

    // Paramètres de spawning de TILES
    [Header("Planet tile settings")]
    private int planetTileCount;
    private int minTilesBetweenDeployPoint;
    private int planetHeatDistance;
    public LayerMask tileLayerMask;
    private PlanetSettings _currentSettings;

    // Informations de la grille de jeu actuelle
    private GridInfo _currentGridInfo;

    // Liste de toutes les planètes générées
    private List<Planet> _allPlanets = new List<Planet>();
    private List<GridTile_Planet> _allPlanetTiles = new List<GridTile_Planet>();
    public List<GridTile_Planet> AllPlanetTiles { get { return _allPlanetTiles; } }

    private int _currentSectorSouls;
    private int _totalSectorSouls;

    // Database pour construire les planètes
    private PlanetTemplateDB _planetTemplateDB;

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
        Ship.soulsFromPlanetSaved += TrackSavedSouls;
        GridTile_Planet.newPlanetTile += OnNewPlanetTileSpawned;
        LevelManager.resetGame += OnGameReset;

        _planetTemplateDB = FindObjectOfType<PlanetTemplateDB>();
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
        GridManager.newGameGrid -= OnNewGameGrid;
        //GridManager.firstAnomalyTile -= OnFirstAnomalyTile;
        Ship.soulsFromPlanetSaved -= TrackSavedSouls;
        GridTile_Planet.newPlanetTile -= OnNewPlanetTileSpawned;
        LevelManager.resetGame -= OnGameReset;
    }

    private void OnGameReset()
    {
        instance = null;
        Destroy(this);
    }

    // Fonction appelée lorsqu'une nouvelle grille est générée qui stock les informations de cette grille
    private void OnNewGameGrid(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
        SpawnPlanetTiles();
        RevealStartingPlanets(_currentSettings);
    }

    public void AssignLevelSettings(PlanetSettings settings)
    {
        planetTileCount = settings.planetTileCount;
        minTilesBetweenDeployPoint = settings.minTilesBetweenDeployPoint;
        planetHeatDistance = settings.planetHeatDistance;
        _currentSettings = settings;
    }

    // Fonction appelée lorsqu'un planète spawn, qui sert à garder à jour la liste de planètes
    private void OnNewPlanetTileSpawned(GridTile_Planet planetTile)
    {
        if (_planetTemplateDB == null)
            _planetTemplateDB = FindObjectOfType<PlanetTemplateDB>();

        _allPlanetTiles.Add(planetTile);

        if (_planetTemplateDB != null)
        {
            PlanetInfo newInfo = new PlanetInfo(_planetTemplateDB.GetRandomPlanetName(), _planetTemplateDB.GetRandomPlanetSpriteMatch());
            planetTile.AssignPlanetInfo(newInfo);
        }
    }

    // Fonction appelée lorsque la grille actuelle est détruite qui reset les variables et références
    private void OnGridDataDestroyed()
    {
        _allPlanets.Clear();
        _allPlanetTiles.Clear();
        _currentGridInfo = null;

        //if (_planetTemplateDB != null)
        //    _planetTemplateDB.ResetDB();
    }

    public void SpawnPlanetTiles()
    {
        if (_currentGridInfo == null)
            return;

        int planetCount = 0;
        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;

        // Get all empty tiles
        List<GridTile> allowedSpawnTiles = new List<GridTile>();
        for (int x = 0; x < _currentGridInfo.gameGridSize.x; x++)
        {
            for (int y = 0; y < _currentGridInfo.gameGridSize.y; y++)
            {
                if (_currentGridInfo.gameGridTiles[x, y].tileType == 0)
                {
                    allowedSpawnTiles.Add(_currentGridInfo.gameGridTiles[x, y]);
                }
            }
        }

        //// Remove tiles too close to deploy point
        //float castRadius = tileSize * minTilesBetweenDeployPoint;

        //foreach (var tile in DeployManager.instance.GetAllDeployTiles())
        //{
        //    Collider2D[] allHits = Physics2D.OverlapCircleAll(tile.TileCenter, castRadius, tileLayerMask);
        //    foreach (var hit in allHits)
        //    {
        //        GridTile candidate = hit.GetComponent<GridTile>();
        //        if (hit != null)
        //        {
        //            allowedSpawnTiles.Remove(candidate);
        //            //Debug.DrawLine(candidate.TileCenter, candidate.TileCenter + Vector2.up * 0.2f, Color.red, 5f);
        //        }
        //    }
        //}

        // Add heat to tiles near map edge
        foreach (var tile in allowedSpawnTiles)
        {
            bool addHeat = false;
            if (tile.tileX == 0 || tile.tileX == _currentGridInfo.gameGridSize.x - 1)
                addHeat = true;
            if (tile.tileY == 0 || tile.tileY == _currentGridInfo.gameGridSize.y - 1)
                addHeat = true;

            // Add heat as if a planet was on this tile
            if (addHeat)
                tile.AddPlanetHeat(1);
        }

        // Spawn planets
        for (int i = 0; i < planetTileCount; i++)
        {
            // Get best matching tiles
            float lowestHeat = float.MaxValue;
            int heatMargin = 1;
            foreach (var tile in allowedSpawnTiles)
            {
                if (tile.PlanetHeat < lowestHeat)
                    lowestHeat = tile.PlanetHeat;
            }

            List<GridTile> lowestHeatTiles = new List<GridTile>();
            foreach (var tile in allowedSpawnTiles)
            {
                if (tile.PlanetHeat <= lowestHeat + heatMargin)
                    lowestHeatTiles.Add(tile);
            }

            if (lowestHeatTiles.Count <= 0)
            {
                Debug.Log("Error in calculating lowest heat tile.");
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, lowestHeatTiles.Count);
            GridTile randomTile = lowestHeatTiles[randomIndex];
            allowedSpawnTiles.Remove(randomTile);

            foreach (var neighbour in randomTile.AllNeighbours)
            {
                allowedSpawnTiles.Remove(neighbour);
            }

            // Add heat, one circle cast per heat distance
            for (int j = 1; j <= planetHeatDistance; j++)
            {
                float heatCastRadius = tileSize * j;

                Collider2D[] allHits = Physics2D.OverlapCircleAll(randomTile.TileCenter, heatCastRadius, tileLayerMask);
                foreach (var hit in allHits)
                {
                    GridTile candidate = hit.GetComponent<GridTile>();
                    if (hit != null)
                    {
                        candidate.AddPlanetHeat(1);
                    }
                }
            }

            if (GridManager.instance != null)
                GridManager.instance.ReplaceTile(randomTile, 3);

            foreach (var tile in _currentGridInfo.gameGridTiles)
            {
                // If same X
                if (tile.tileX == randomTile.tileX)
                    // If not exact same tile
                    if (tile.tileY != randomTile.tileY)
                    {
                        // If neighbour
                        if (tile.tileY == randomTile.tileY - 1 || tile.tileY == randomTile.tileY + 1)
                            tile.AddPlanetHeat(2);
                        // If other
                        else
                            tile.AddPlanetHeat(1);
                    }
                        
                if (tile.tileY == randomTile.tileY)
                    // If not exact same tile
                    if (tile.tileX != randomTile.tileX)
                    {
                        // If neighbour
                        if (tile.tileX == randomTile.tileX - 1 || tile.tileX == randomTile.tileX + 1)
                            tile.AddPlanetHeat(2);
                        // If other
                        else
                            tile.AddPlanetHeat(1);
                    }
            }
            planetCount++;
        }
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

    private void TrackSavedSouls(PlanetSoulsMatch match)
    {
        Debug.Log("Received match with souls : " + match.soulsAmount);

        if (!_allPlanets.Contains(match.linkedPlanet))
        {
            Debug.LogError("Could not find linked planet when unloading souls.");
            return;
        }

        match.linkedPlanet.OnSoulsSaved(match.soulsAmount);
    }

    public void TogglePlanetDebug(bool toggleON)
    {
        foreach (var planet in _allPlanets)
        {
            planet.ToggleSprite(toggleON);
        }
    }

    public void RevealStartingPlanets(PlanetSettings settings)
    {
        int totalPlanetsRevealed = 0;

        int planetCount = _allPlanetTiles.Count;

        //int totalDistance = 0;
        //int averageDistance = 0;

        //foreach (var planet in _allPlanetTiles)
        //{
        //    totalDistance += planet.DistanceRating;
        //}
        //averageDistance = totalDistance / planetCount;

        //Debug.Log("Average distance : " + averageDistance);

        List<GridTile_Planet> allCandidatePlanets = new List<GridTile_Planet>(_allPlanetTiles);
        List<GridTile_Planet> closePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> farPlanets = new List<GridTile_Planet>();

        //foreach (var planet in _allPlanetTiles)
        //{
        //    if (planet.DistanceRating >= averageDistance)
        //        farPlanets.Add(planet);
        //    else
        //        closePlanets.Add(planet);
        //}

        int closeCount = closePlanets.Count;
        int farCount = farPlanets.Count;

        for (int i = 0; i < settings.planetsRevealedCloseToDeploy; i++)
        {
            if (i < closeCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, closeCount);
                closePlanets[randomIndex].RevealPlanet(true);

                //Debug.Log("Close planet chosen : " + closePlanets[randomIndex].DistanceRating);

                allCandidatePlanets.Remove(closePlanets[randomIndex]);
                closePlanets.RemoveAt(randomIndex);
                closeCount--;
                totalPlanetsRevealed++;
            }
            else
                break;
        }
        for (int i = 0; i < settings.planetsRevealedFarFromDeploy; i++)
        {
            if (i < farCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, farCount);
                farPlanets[randomIndex].RevealPlanet(true);

                //Debug.Log("Far planet chosen : " + farPlanets[randomIndex].DistanceRating);

                allCandidatePlanets.Remove(farPlanets[randomIndex]);
                farPlanets.RemoveAt(randomIndex);
                farCount--;
                totalPlanetsRevealed++;
            }
            else
                break;
        }
        while (totalPlanetsRevealed < settings.planetsRevealedOnStart)
        {
            int allCandidatesCount = allCandidatePlanets.Count;
            if (allCandidatesCount == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, allCandidatesCount);
            allCandidatePlanets[randomIndex].RevealPlanet(true);
            allCandidatePlanets.RemoveAt(randomIndex);
            totalPlanetsRevealed++;
        }
    }

    public List<GridTile_Planet> GetRandomPlanetMatch()
    {
        List<GridTile_Planet> availablePlanets = new List<GridTile_Planet>(_allPlanetTiles);
        List<GridTile_Planet> planetMatch = new List<GridTile_Planet>();
        int count = _allPlanetTiles.Count;

        int randomIndex = UnityEngine.Random.Range(0, count);
        planetMatch.Add(availablePlanets[randomIndex]);
        availablePlanets.RemoveAt(randomIndex);
        count--;

        randomIndex = UnityEngine.Random.Range(0, count);
        planetMatch.Add(availablePlanets[randomIndex]);
        availablePlanets.RemoveAt(randomIndex);
        count--;

        return planetMatch;
    }

    public GridTile_Planet PlanetAt(TileCoordinates tileCoords)
    {
        foreach (var p in _allPlanetTiles)
        {
            if (p.tileX == tileCoords.tileX && p.tileY == tileCoords.tileY)
                return p;
        }
        return null;
    }
}

[System.Serializable]
public struct PlanetArchetypeQuantity
{
    public PlanetArchetype archetype;
    public int amountToSpawn;
}

public struct PlanetInfo
{
    public string planetName;
    public PlanetSpriteMatch spriteMatch;

    public PlanetInfo(string planetName, PlanetSpriteMatch spriteMatch)
    {
        this.planetName = planetName;
        this.spriteMatch = spriteMatch;
    }
}
