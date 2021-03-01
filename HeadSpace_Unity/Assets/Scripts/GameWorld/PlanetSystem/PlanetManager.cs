using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    // Singleton
    public static PlanetManager instance;

    // Paramètres de spawning et archétypes de planètes
    [Header("Random population settings")]
    public bool randomArchetypes;
    public int randomPlanetCount;

    [Header("Archetype population settings")]
    public List<PlanetArchetypeQuantity> allArchetypes;

    // Prefab
    public GameObject planetPrefab;

    // Informations de la grille de jeu actuelle
    private GridInfo _currentGridInfo;

    // Liste de toutes les planètes générées
    private List<Planet> _allPlanets = new List<Planet>();

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
        GridManager.firstAnomalyTile += OnFirstAnomalyTile;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
        GridManager.newGameGrid -= OnNewGameGrid;
        GridManager.firstAnomalyTile -= OnFirstAnomalyTile;
    }

    // Fonction appelée lorsqu'une nouvelle grille est générée qui stock les informations de cette grille
    private void OnNewGameGrid(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
    }

    // Fonction appelée lorsque l'anomalie a été assignée, afin d'assigner par la suite les planètes
    private void OnFirstAnomalyTile()
    {
        SpawnPlanets();
    }

    // Fonction appelée lorsqu'un planète spawn, qui sert à garder à jour la liste de planètes
    private void OnNewPlanetSpawned(Planet planet)
    {
        _allPlanets.Add(planet);
    }

    // Fonction appelée lorsque la grille actuelle est détruite qui reset les variables et références
    private void OnGridDataDestroyed()
    {
        _allPlanets.Clear();
        _currentGridInfo = null;
    }

    private void SpawnPlanets()
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
                    int randomIndex = Random.Range(0, archetypeCount);
                    archetypesToSpawn.Add(allArchetypes[randomIndex].archetype);
                }
            }
            else
            {
                for (int i = 0; i < planetCount; i++)
                {
                    string planetName = "randomizedPlanet";
                    int randomMinPopulation = Random.Range(0, 15);
                    int randomMaxPopulation = Random.Range(randomMinPopulation + 1, 31);
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
            int randomIndex = Random.Range(0, allowedSpawnTiles.Count);
            Vector2 spawnPos = GridCoords.GetRandomCoordsInTile(allowedSpawnTiles[randomIndex]);
            Planet planet = Instantiate(planetPrefab).GetComponent<Planet>();
            planet.PlaceGridObject(spawnPos);
            planet.AssignArchetype(archetypesToSpawn[i]);
            _allPlanets.Add(planet);
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
            Debug.Log("yo");
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

    public void TogglePlanetDebug()
    {
        foreach (var planet in _allPlanets)
        {
            planet.ToggleSprite();
        }
    }

}

[System.Serializable]
public struct PlanetArchetypeQuantity
{
    public PlanetArchetype archetype;
    public int amountToSpawn;
}
