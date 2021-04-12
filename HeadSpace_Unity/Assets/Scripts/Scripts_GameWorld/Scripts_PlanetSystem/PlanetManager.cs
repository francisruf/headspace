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

    // Paramètres de spawning et archétypes de planètes
    [Header("Random population settings")]
    public bool randomArchetypes;
    public int randomPlanetCount;

    // Paramètres de spawning de TILES
    [Header("Planet tile settings")]
    public int planetTileCount;
    public int minTilesBetweenDeployPoint;
    public int planetHeatDistance;
    public LayerMask tileLayerMask;

    [Header("Archetype population settings")]
    public List<PlanetArchetypeQuantity> allArchetypes;

    [Header("Prefab")]
    // Prefab
    public GameObject planetPrefab;

    // Informations de la grille de jeu actuelle
    private GridInfo _currentGridInfo;

    // Liste de toutes les planètes générées
    private List<Planet> _allPlanets = new List<Planet>();
    private List<GridTile_Planet> _allPlanetTiles = new List<GridTile_Planet>();

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

        _planetTemplateDB = GetComponentInChildren<PlanetTemplateDB>();
    }

    // Subscription à différentes actions
    private void OnEnable()
    {
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
        GridManager.newGameGrid += OnNewGameGrid;
        //GridManager.firstAnomalyTile += OnFirstAnomalyTile;
        Ship.soulsFromPlanetSaved += TrackSavedSouls;
        Ship.soulsUnloaded += OnSoulsUnloaded;
        Planet.soulsLost += OnSoulsLost;
        GridTile_Planet.newPlanetTile += OnNewPlanetTileSpawned;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
        GridManager.newGameGrid -= OnNewGameGrid;
        //GridManager.firstAnomalyTile -= OnFirstAnomalyTile;
        Ship.soulsFromPlanetSaved -= TrackSavedSouls;
        Ship.soulsUnloaded -= OnSoulsUnloaded;
        Planet.soulsLost -= OnSoulsLost;
        GridTile_Planet.newPlanetTile -= OnNewPlanetTileSpawned;
    }

    // Fonction appelée lorsqu'une nouvelle grille est générée qui stock les informations de cette grille
    private void OnNewGameGrid(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
    }

    // Fonction appelée lorsque l'anomalie a été assignée, afin d'assigner par la suite les planètes
    //private void OnFirstAnomalyTile()
    //{
    //    SpawnPlanets();
    //}

    // Fonction appelée lorsqu'un planète spawn, qui sert à garder à jour la liste de planètes
    private void OnNewPlanetSpawned(Planet planet)
    {
        //_allPlanets.Add(planet);
    }

    // Fonction appelée lorsqu'un planète spawn, qui sert à garder à jour la liste de planètes
    private void OnNewPlanetTileSpawned(GridTile_Planet planetTile)
    {
        _allPlanetTiles.Add(planetTile);

        if (_planetTemplateDB != null)
        {
            PlanetInfo newInfo = new PlanetInfo(_planetTemplateDB.GetRandomPlanetName(), _planetTemplateDB.GetRandomPlanetSprite());
            planetTile.AssignPlanetInfo(newInfo);
        }
    }

    // Fonction appelée lorsque la grille actuelle est détruite qui reset les variables et références
    private void OnGridDataDestroyed()
    {
        _allPlanets.Clear();
        _allPlanetTiles.Clear();
        _currentGridInfo = null;

        if (_planetTemplateDB != null)
            _planetTemplateDB.ResetDB();
    }

    public void SpawnPlanetTiles()
    {
        if (_currentGridInfo == null)
            return;

        int planetCount = 0;

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

        // Remove tiles too close to deploy point
        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;
        float castRadius = tileSize * minTilesBetweenDeployPoint;

        foreach (var tile in DeployManager.instance.GetAllDeployTiles())
        {
            Collider2D[] allHits = Physics2D.OverlapCircleAll(tile.TileCenter, castRadius, tileLayerMask);
            foreach (var hit in allHits)
            {
                GridTile candidate = hit.GetComponent<GridTile>();
                if (hit != null)
                {
                    allowedSpawnTiles.Remove(candidate);
                    Debug.DrawLine(candidate.TileCenter, candidate.TileCenter + Vector2.up * 0.2f, Color.red, 5f);
                }
            }
        }

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
                tile.AddPlanetHeat(planetHeatDistance / 2);
        }

        // Spawn planets
        for (int i = 0; i < planetTileCount; i++)
        {
            // Get best matching tiles
            float lowestHeat = float.MaxValue;
            int heatMargin = 2;
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

            planetCount++;
        }
    }

    public void SpawnPlanets()
    {
        if (_currentGridInfo == null)
            return;

        int planetCount = 0;
        List<PlanetArchetype> archetypesToSpawn = new List<PlanetArchetype>();

        if (randomArchetypes)
        {
            planetCount = randomPlanetCount;
            int archetypeCount = allArchetypes.Count;

            if (archetypeCount > 0)
            {
                for (int i = 0; i < planetCount; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, archetypeCount);
                    archetypesToSpawn.Add(allArchetypes[randomIndex].archetype);
                }
            }
            else
            {
                for (int i = 0; i < planetCount; i++)
                {
                    string planetName = "randomizedPlanet";
                    int randomMinPopulation = UnityEngine.Random.Range(0, 15);
                    int randomMaxPopulation = UnityEngine.Random.Range(randomMinPopulation + 1, 31);
                    int creditsBonus = (randomMinPopulation + randomMaxPopulation / 2) / 10;

                    archetypesToSpawn.Add(new PlanetArchetype(planetName, randomMinPopulation, randomMaxPopulation, creditsBonus));
                }
            }
        }
        else
        {
            foreach (var archetype in allArchetypes)
            {
                for (int i = 0; i < archetype.amountToSpawn; i++)
                {
                    archetypesToSpawn.Add(archetype.archetype);
                }
                planetCount += archetype.amountToSpawn;
            }
        }


        List<GridTile> allowedSpawnTiles = GetAllowedSpawnTiles();

        for (int i = 0; i < planetCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, allowedSpawnTiles.Count);
            Vector2 spawnPos = GridCoords.GetRandomCoordsInTile(allowedSpawnTiles[randomIndex]);
            Planet planet = Instantiate(planetPrefab).GetComponent<Planet>();
            planet.PlaceGridObject(spawnPos);
            planet.AssignArchetype(archetypesToSpawn[i]);
            _allPlanets.Add(planet);
        }

        foreach (var planet in _allPlanets)
        {
            _totalSectorSouls += planet.TotalSouls;
        }
        _currentSectorSouls = _totalSectorSouls;

        if (planetsSpawned != null)
            planetsSpawned(_allPlanets);

        //Debug.Log("Planet count : " + _allPlanets.Count);
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

    private void OnSoulsLost(Planet planet, int amount)
    {
        _currentSectorSouls -= amount;
        CheckForEndCondition();
    }

    private void OnSoulsUnloaded(int amount)
    {
        _currentSectorSouls -= amount;
        CheckForEndCondition();
    }

    private void CheckForEndCondition()
    {
        if (_currentSectorSouls <= 0)
        {
            if (noMoreSoulsInSector != null)
                noMoreSoulsInSector();
        }
    }
    
    public void RevealStartingPlanets(PlanetSettings settings)
    {
        
        int totalPlanetsRevealed = 0;

        int planetCount = _allPlanetTiles.Count;

        int totalDistance = 0;
        int averageDistance = 0;

        foreach (var planet in _allPlanetTiles)
        {
            totalDistance += planet.DistanceRating;
        }
        averageDistance = totalDistance / planetCount;

        //Debug.Log("Average distance : " + averageDistance);

        List<GridTile_Planet> allCandidatePlanets = new List<GridTile_Planet>(_allPlanetTiles);
        List<GridTile_Planet> closePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> farPlanets = new List<GridTile_Planet>();

        foreach (var planet in _allPlanetTiles)
        {
            if (planet.DistanceRating >= averageDistance)
                farPlanets.Add(planet);
            else
                closePlanets.Add(planet);
        }

        int closeCount = closePlanets.Count;
        int farCount = farPlanets.Count;
        //Debug.Log("0");
        for (int i = 0; i < settings.planetsRevealedCloseToDeploy; i++)
        {
            if (i < closeCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, closeCount);
                closePlanets[randomIndex].RevealPlanet();

                //Debug.Log("Close planet chosen : " + closePlanets[randomIndex].DistanceRating);

                allCandidatePlanets.Remove(closePlanets[randomIndex]);
                closePlanets.RemoveAt(randomIndex);
                closeCount--;
                totalPlanetsRevealed++;
            }
            else
                break;
        }
        //Debug.Log("1");
        for (int i = 0; i < settings.planetsRevealedFarFromDeploy; i++)
        {
            if (i < farCount)
            {
                int randomIndex = UnityEngine.Random.Range(0, farCount);
                farPlanets[randomIndex].RevealPlanet();

                //Debug.Log("Far planet chosen : " + farPlanets[randomIndex].DistanceRating);

                allCandidatePlanets.Remove(closePlanets[randomIndex]);
                farPlanets.RemoveAt(randomIndex);
                farCount--;
                totalPlanetsRevealed++;
            }
            else
                break;
        }
        //Debug.Log("2");
        while (totalPlanetsRevealed < settings.planetsRevealedOnStart)
        {
            int allCandidatesCount = allCandidatePlanets.Count;
            if (allCandidatesCount == 0)
                break;

            int randomIndex = UnityEngine.Random.Range(0, allCandidatesCount);
            allCandidatePlanets[randomIndex].RevealPlanet();
            allCandidatePlanets.RemoveAt(randomIndex);
            totalPlanetsRevealed++;
        }
        //Debug.Log("3");
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
    public Sprite planetSprite;

    public PlanetInfo(string planetName, Sprite planetSprite)
    {
        this.planetName = planetName;
        this.planetSprite = planetSprite;
    }
}
