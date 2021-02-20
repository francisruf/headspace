using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    // ACTION / EVENT qui est appelé à chaque fois qu'une grille est créée, qui envoie les informations de cette grille
    public static Action<GridInfo> newGameGrid;

    // Singleton
    public static GridManager instance;

    // Paramètres de la grille
    [Header("Grid settings")]
    public int mapSizeX;
    public int mapSizeY;
    public float mapWidth;

    [Header("Anomaly settings")]
    public int[] newSegmentAfterCount;

    [Header("Tile prefabs")]
    // Prefabs de tuiles et objets
    public GameObject emptyCellPrefab;
    public GameObject anomaly0Prefab;
    public GameObject anomaly1Prefab;
    public GameObject anomaly2Prefab;

    [Header("Object prefabs")]
    public GameObject deployPointPrefab;

    //[Header("Deploy point settings")]
    //public Vector2 deployStartCoordinates;

    // La grille
    private int[,] _gameGrid;   // Array2D d'ints pour génération initiale et pathfinding
    private GridTile[,] _gameGridTiles;    // Array 2D pour la grille de TUILES physiques

    // Informations de la grille actuelle (voir Struct en bas de cette classe)
    private GridInfo _currentGridInfo;

    // Liste d'objets faisant partie de la grille
    private List<GridStaticObject> _allStaticObjects = new List<GridStaticObject>();

    // Segments de l'anomalie
    private List<AnomalySegment> _allAnomalySegments = new List<AnomalySegment>();
    private int _anomalyCompletedTileCount;

    // Subscription aux ACTIONS d'autres classes
    private void OnEnable()
    {
        GridTile.anomalyTileComplete += OnNewAnomalyTileComplete;
        GridStaticObject.gridObjectPositionAdded += OnGridObjectPositionAdded;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridTile.anomalyTileComplete -= OnNewAnomalyTileComplete;
        GridStaticObject.gridObjectPositionAdded -= OnGridObjectPositionAdded;
    }

    private void Awake()
    {
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

    private void Update()
    {

        // TEMP : Génération de nouvelle grille on KeyDown
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_gameGridTiles != null)
                DestroyMapData();

            GenerateMapData();
            GenerateMapTiles();

            if (newGameGrid != null)
                newGameGrid(_currentGridInfo);

            GenerateStartingObjects();
            Array.Sort(newSegmentAfterCount);
        }
    }

    // Fonction qui détruit l'ancienne grille, si elle existe
    private void DestroyMapData()
    {
        foreach (var gridTile in _gameGridTiles)
        {
            Destroy(gridTile.gameObject);
        }

        foreach (var obj in _allStaticObjects)
        {
            obj.DisableGridObject();
        }

        foreach (var obj in _allStaticObjects)
        {
            Destroy(obj.gameObject);
        }

        _allStaticObjects.Clear();
    }

    // Fonction qui génère la grille d'entiers
    private void GenerateMapData()
    {
        // Assigner la grosseur des arrays2D
        _gameGrid = new int[mapSizeX, mapSizeY];
        _gameGridTiles = new GridTile[mapSizeX, mapSizeY];

        // Assigner la valeur "0" à chaque index du array
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                _gameGrid[x, y] = 0;
            }
        }
    }

    /* FONCTION DE GÉNÉRATION DE TUILES
     * - Génère une grille de tuiles physiques dans le world
     * - Assigne les dimensions d'une tuile pour que la grille reste confinée en tout temps dans l'espace de jeu
     * - La largeur (mapWidth) détermine la grosseur des tuiles, en fonction du nombre de tuiles en X
     */
    private void GenerateMapTiles()
    {
        float tileWidth = mapWidth / mapSizeX;   // Dimensions d'une tuile en unités de unity
        Vector3 spawnOffset = new Vector3(mapWidth / 2f, (tileWidth * mapSizeY) / 2, 0f);

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                // Instanciation des tuiles, à partir du array 2D de ints (voir fonction suivante)
                GameObject tilePrefab = GetTileFromID(_gameGrid[x, y]);
                Vector3 spawnPos = new Vector3(x * tileWidth, y * tileWidth, 0f);
                spawnPos -= spawnOffset;

                // Assignation des tuiles et initialisation
                GameObject go = Instantiate(tilePrefab, spawnPos, Quaternion.identity);

                // TEMP Instantiation sous le DebugManager
                if (DebugManager.instance != null)
                {
                    go.transform.SetParent(DebugManager.instance.gridDebug.transform);
                }

                // Assignation des paramètres sur les OBJETS de tuile
                GridTile gt = go.GetComponent<GridTile>();
                gt.tileX = x;
                gt.tileY = y;
                gt.tileType = _gameGrid[x, y];
                gt.InitializeTile(new Vector2(tileWidth, tileWidth));

                // Ajout de la tuile à l'array2D de tuiles
                _gameGridTiles[x, y] = gt;
            }
        }

        // Assignation des informations de la grille et appel de l'ACTION de nouvelle grille
        Bounds newBounds = new Bounds(Vector3.zero, new Vector3(mapWidth, tileWidth * mapSizeY, 0f));
        _currentGridInfo = new GridInfo(_gameGridTiles, new Vector2(mapSizeX, mapSizeY), newBounds);
    }

    // Fonction qui génère les premiers objets sur la grille (points de déploiement / planètes(TBD))
    private void GenerateStartingObjects()
    {
        // ------ DEPLOY POINTS -------

        GameObject go = Instantiate(deployPointPrefab);

        // TEMP Instantiation sous le DebugManager
        if (DebugManager.instance != null)
        {
            go.transform.SetParent(DebugManager.instance.gridDebug.transform);
        }

        // Placer le Deploy point en fonction d'un cadran aléatoire
        GridStaticObject obj = go.GetComponent<GridStaticObject>();
        GridQuadrants.QuadrantMatch quadrantMatch = _currentGridInfo.gameGridQuadrants.GetRandomQuadrantMatch(out _currentGridInfo.positiveQuadrantIndex);
        
        Vector3 spawnPos = RandomPointInBounds(quadrantMatch.positiveQuadrant);
        spawnPos = GridCoords.FromWorldToGrid(spawnPos);

        obj.PlaceGridObject(spawnPos);

        // ------ ANOMALY ------

        // Placer l'anomalie selon un point au hasard dans le cadran opposé
        Vector3 randomAnomalyPoint = RandomPointInBounds(quadrantMatch.negativeQuadrant);
        Debug.Log("RANDOM NEGATIVE POINT : " + randomAnomalyPoint);
        TileCoordinates startTile = GridCoords.FromWorldToTile(randomAnomalyPoint);

        InstantiateAnomalySegment(startTile);
    }

    private void InstantiateAnomalySegment(TileCoordinates startTile)
    {
        Debug.Log("X : " + startTile.tileX + "Y : " + startTile.tileY);
        GridTile tileToReplace = _gameGridTiles[startTile.tileX, startTile.tileY];
        GridTile newTile;
        ReplaceTile(tileToReplace, 1, out newTile);

        AnomalySegment segment = new AnomalySegment();
        _allAnomalySegments.Add(segment);
        segment.AssignActiveTile(newTile);
    }

    private void InstantiateAnomalySegment(int positiveQuadrantIndex)
    {
        TileCoordinates newAnomalyCoords = default;
        List<GridTile> candidates = new List<GridTile>();
        List<Bounds> otherQuadrants = new List<Bounds>(_currentGridInfo.gameGridQuadrants.GetOtherBounds(_currentGridInfo.positiveQuadrantIndex));

        bool foundPosition = false;
        while (!foundPosition)
        {
            if (otherQuadrants.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, otherQuadrants.Count);
                Bounds candidateBounds = otherQuadrants[randomIndex];
                otherQuadrants.Remove(candidateBounds);

                candidates = GetGridTilesInBounds(candidateBounds);
                foreach (var candidate in candidates)
                {
                    if (candidate.tileType == 0)
                    {
                        newAnomalyCoords.tileX = candidate.tileX;
                        newAnomalyCoords.tileY = candidate.tileY;
                        foundPosition = true;
                        break;
                    }
                }
            }
            else
            {
                foundPosition = true;
                Debug.Log("NO MORE POSSIBLE TILES TO SPAWN ANOMALY");
                return;
            }

        }

        Debug.Log("X : " + newAnomalyCoords.tileX + "Y : " + newAnomalyCoords.tileY);
        GridTile tileToReplace = _gameGridTiles[newAnomalyCoords.tileX, newAnomalyCoords.tileY];
        GridTile newTile;
        ReplaceTile(tileToReplace, 1, out newTile);

        AnomalySegment segment = new AnomalySegment();
        _allAnomalySegments.Add(segment);
        segment.AssignActiveTile(newTile);
    }

    // Fonction utilitaire qui donne le bon prefab de tuile, selon le int dans l'array2D
    private GameObject GetTileFromID(int tileID)
    {
        switch (tileID)
        {
            case 0:
                {
                    return emptyCellPrefab;
                }

            case 1:
                {
                    return anomaly0Prefab;
                }

            case 2:
                {
                    return anomaly1Prefab;
                }

            case 3:
                {
                    return anomaly2Prefab;
                }

            default:
                {
                    return emptyCellPrefab;
                }
        }
    }

    public void ReplaceTile(GridTile deadTile, int newTileType)
    {
        float tileWidth = deadTile.tileDimensions.x;

        // Assignation du bon prefab et spawnPos
        GameObject tilePrefab = GetTileFromID(newTileType);
        Vector3 spawnPos = deadTile.transform.position;

        // Instantiation
        GameObject go = Instantiate(tilePrefab, spawnPos, Quaternion.identity);

        // TEMP Instantiation sous le DebugManager
        if (DebugManager.instance != null)
        {
            go.transform.SetParent(DebugManager.instance.gridDebug.transform);
        }

        // Assignation des paramètres sur les OBJETS de tuile
        GridTile gt = go.GetComponent<GridTile>();
        gt.tileX = deadTile.tileX;
        gt.tileY = deadTile.tileY;
        gt.tileType = newTileType;
        gt.InitializeTile(deadTile.tileDimensions, _currentGridInfo);

        gt.TransferObjectList(deadTile.CurrentObjectsInTile);

        // Remplacement de la tuile dans les arrays2D
        _gameGrid[deadTile.tileX, deadTile.tileY] = newTileType;
        _gameGridTiles[deadTile.tileX, deadTile.tileY] = gt;

        deadTile.TriggerNeighbourTilesUpdates();
        deadTile.DisableTile();
    }

    // Fonction qui remplace une tuile existante par un nouveau type de tuile
    public void ReplaceTile(GridTile deadTile, int newTileType, out GridTile newTile)
    {
        float tileWidth = deadTile.tileDimensions.x;

        // Assignation du bon prefab et spawnPos
        GameObject tilePrefab = GetTileFromID(newTileType);
        Vector3 spawnPos = deadTile.transform.position;

        // Instantiation
        GameObject go = Instantiate(tilePrefab, spawnPos, Quaternion.identity);

        // TEMP Instantiation sous le DebugManager
        if (DebugManager.instance != null)
        {
            go.transform.SetParent(DebugManager.instance.gridDebug.transform);
        }

        // Assignation des paramètres sur les OBJETS de tuile
        newTile = go.GetComponent<GridTile>();
        newTile.tileX = deadTile.tileX;
        newTile.tileY = deadTile.tileY;
        newTile.tileType = newTileType;
        newTile.InitializeTile(deadTile.tileDimensions, _currentGridInfo);

        newTile.TransferObjectList(deadTile.CurrentObjectsInTile);

        // Remplacement de la tuile dans les arrays2D
        _gameGrid[deadTile.tileX, deadTile.tileY] = newTileType;
        _gameGridTiles[deadTile.tileX, deadTile.tileY] = newTile;

        deadTile.TriggerNeighbourTilesUpdates();
        deadTile.DisableTile();
    }

    // Fonction qui remplace une tuile existante par un nouveau type de tuile
    public void ReplaceTile(TileCoordinates deadTileCoords, int newTileType, out GridTile newTile)
    {
        GridTile deadTile = _gameGridTiles[deadTileCoords.tileX, deadTileCoords.tileY];
        float tileWidth = deadTile.tileDimensions.x;

        // Assignation du bon prefab et spawnPos
        GameObject tilePrefab = GetTileFromID(newTileType);
        Vector3 spawnPos = deadTile.transform.position;

        // Instantiation
        GameObject go = Instantiate(tilePrefab, spawnPos, Quaternion.identity);

        // TEMP Instantiation sous le DebugManager
        if (DebugManager.instance != null)
        {
            go.transform.SetParent(DebugManager.instance.gridDebug.transform);
        }

        // Assignation des paramètres sur les OBJETS de tuile
        newTile = go.GetComponent<GridTile>();
        newTile.tileX = deadTile.tileX;
        newTile.tileY = deadTile.tileY;
        newTile.tileType = newTileType;
        newTile.InitializeTile(deadTile.tileDimensions, _currentGridInfo);

        newTile.TransferObjectList(deadTile.CurrentObjectsInTile);

        // Remplacement de la tuile dans les arrays2D
        _gameGrid[deadTile.tileX, deadTile.tileY] = newTileType;
        _gameGridTiles[deadTile.tileX, deadTile.tileY] = newTile;

        deadTile.TriggerNeighbourTilesUpdates();
        deadTile.DisableTile();
    }

    // Fonction qui track la quantité de tuiles d'anomalie et qui trigger le spawn des prochains segments
    public void OnNewAnomalyTileComplete(GridTile tile)
    {
        _anomalyCompletedTileCount++;

        int anomalyStepCount = newSegmentAfterCount.Length;
        if (_allAnomalySegments.Count <= anomalyStepCount)
        {
            if (_anomalyCompletedTileCount == newSegmentAfterCount[_allAnomalySegments.Count - 1])
            {
                InstantiateAnomalySegment(_currentGridInfo.positiveQuadrantIndex);
            }
        }
    }

    // Fonction appelée par l'action dans la classe GridStaticObject
    // Permet d'ajouter un objet à la liste et ajouter à la tuile qui le contient
    private void OnGridObjectPositionAdded(GridStaticObject obj)
    {
        if (obj.ParentTile.tileX < 0 || obj.ParentTile.tileX >= _currentGridInfo.gameGridSize.x)
        {
            Debug.Log("INVALID OBJECT X POSITION");
            return;
        }

        if (obj.ParentTile.tileY < 0 || obj.ParentTile.tileY >= _currentGridInfo.gameGridSize.y)
        {
            Debug.Log("INVALID OBJECT Y POSITION");
            return;
        }

        // Ajout de l'objet à la liste et subscribe à son Action Remove
        _allStaticObjects.Add(obj);
        obj.gridObjectPositionRemoved += OnGridObjectPositionRemoved;

        // Ajout de l'objet aux données de la tuile qui le contient
        _gameGridTiles[obj.ParentTile.tileX, obj.ParentTile.tileY].AddObjectToTile(obj);
    }

    // Enlever un objet de la liste d'objets
    private void OnGridObjectPositionRemoved(GridStaticObject obj)
    {
        obj.gridObjectPositionRemoved -= OnGridObjectPositionRemoved;
    }

    // Fonction DEBUG qui change une tuile au hasard par une anomalie
    public void SpawnRandomAnomalyTile()
    {
        int tileX = UnityEngine.Random.Range(0, mapSizeX);
        int tileY = UnityEngine.Random.Range(0, mapSizeY);

        ReplaceTile(_gameGridTiles[tileX, tileY], 1);
    }

    // Fonction utilitaire pour obtenir un point au hasard à l'intérieur de BOUNDS
    public Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            bounds.center.z
        );
    }

    public List<GridTile> GetGridTilesInBounds(Bounds bounds)
    {
        List<GridTile> tilesInBounds = new List<GridTile>();
        foreach (var gridTile in _gameGridTiles)
        {
            if (bounds.Contains(gridTile.transform.position))
            {
                tilesInBounds.Add(gridTile);
            }
        }
        return tilesInBounds;
    }
}

/* STRUCT : 
 * - Un struct est une STRUCTURE de données semblable à une classe.
 * - Ce struct permet de mémoriser plusieurs variables dans un même contenant.
 */
public struct TileCoordinates
{
    public int tileX;
    public int tileY;

    public TileCoordinates(int tileX, int tileY)
    {
        this.tileX = tileX;
        this.tileY = tileY;
    }
}

