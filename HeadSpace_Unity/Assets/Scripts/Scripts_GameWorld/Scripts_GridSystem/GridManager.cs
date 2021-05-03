using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridCoords))]
public class GridManager : MonoBehaviour
{
    // ACTION / EVENT qui est appelé à chaque fois qu'une grille est créée, qui envoie les informations de cette grille
    public static Action<GridInfo> newGameGrid;
    public static Action totalGridAnomaly;
    public static Action gridDataDestroyed;
    public static Action<GridMode> newGridMode;

    // Singleton
    public static GridManager instance;

    // Référence aux GridCoords
    private GridCoords _gridCoords;
    private PathFinder _pathFinder;

    private GridSettings _currentSettings;
    // Paramètres de la grille
    [Header("Grid settings")]
    private int mapSizeX;
    private int mapSizeY;

    [Header("StartTileMap prefabs")]
    private List<GameObject> tilemapPrefabs;

    [Header("Map width if no WorldMap found")]
    public float mapWidth;

    [Header("Static anomaly settings")]
    private float anomalySpreadTime;

    //[Header("Anomaly settings")]
    //public int[] newSegmentAfterCount;

    [Header("Tile prefabs")]
    // Prefabs de tuiles et objets
    public GameObject emptyCellPrefab;
    public GameObject deployTilePrefab;
    public GameObject rocktilePrefab;
    public GameObject planetTilePrefab;
    public GameObject staticAnomalyPrefab;
    public GameObject spawningStaticAnomalyPrefab;
    public GameObject anomaly0Prefab;
    public GameObject anomaly1Prefab;
    public GameObject anomaly2Prefab;

    // La grille
    private int[,] _gameGrid;   // Array2D d'ints pour génération initiale et pathfinding
    private GridTile[,] _gameGridTiles;    // Array 2D pour la grille de TUILES physiques
    private GridMode _currentGridMode;

    // Informations de la grille actuelle (voir Struct en bas de cette classe)
    private GridInfo _currentGridInfo;

    // Liste d'objets faisant partie de la grille
    private List<GridStaticObject> _allStaticObjects = new List<GridStaticObject>();

    // Segments de l'anomalie
    private List<AnomalySegment> _allAnomalySegments = new List<AnomalySegment>();
    private int _anomalyCompletedTileCount;

    // Nouveaux segments d'anomalie
    private List<AnomalyPatch> _allAnomalyPatches = new List<AnomalyPatch>();
    private IEnumerator _currentSpreadingRoutine;

    // Référence à la world map pour assigner la grosseur de la grille
    private DynamicWorldMap _worldMap;

    // Subscription aux ACTIONS d'autres classes
    private void OnEnable()
    {
        GridStaticObject.gridObjectPositionAdded += OnGridObjectPositionAdded;
    }

    // Unsubscription
    private void OnDisable()
    {
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

        _gridCoords = GetComponent<GridCoords>();
        _pathFinder = GetComponent<PathFinder>();
    }

    private void Start()
    {
        _currentGridMode = GridMode.WorldMap;
        //GenerateNewGrid();
    }

    public void AssignSettings(GridSettings settings)
    {
        mapSizeX = settings.mapSizeX;
        mapSizeY = settings.mapSizeY;
        tilemapPrefabs = settings.tilemapPrefabs;
        anomalySpreadTime = settings.anomalySpreadTime;

        _currentSettings = settings;
    }

    public void GenerateNewGrid()
    {
        if (_gameGridTiles != null)
            DestroyMapData();

        _worldMap = FindObjectOfType<DynamicWorldMap>();

        GenerateMapData();
        GenerateMapTiles();
        GenerateEnvironment();
        GenerateDeployPoint();
        //GenerateStartingObjects();
        //Array.Sort(newSegmentAfterCount);

        _pathFinder.AssignGridInfo(_currentGridInfo);

        if (newGameGrid != null)
            newGameGrid(_currentGridInfo);

        _gridCoords.AssignGridInfo(_currentGridInfo);

        _currentSpreadingRoutine = AnomalySpreadingTimer();
        StartCoroutine(_currentSpreadingRoutine);

        Debug.Log("New grid generated");
    }

    // Fonction qui détruit l'ancienne grille, si elle existe
    private void DestroyMapData()
    {
        foreach (var gridTile in _gameGridTiles)
        {
            Destroy(gridTile.gameObject);
        }

        _allStaticObjects.Clear();
        _allAnomalySegments.Clear();
        _allAnomalyPatches.Clear();
        _anomalyCompletedTileCount = 0;
        _worldMap = null;

        if (_currentSpreadingRoutine != null)
        {
            StopCoroutine(_currentSpreadingRoutine);
            _currentSpreadingRoutine = null;
        }

        if (gridDataDestroyed != null)
            gridDataDestroyed();
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
        float actualWidth = mapWidth;
        
        // Assigner le size de la grille par rapport à la grosseur de la world map, si elle est trouvable
        if (_worldMap != null)
        {
            actualWidth = _worldMap.GetComponent<SpriteRenderer>().size.x;
        }

        float tileWidth = 0.03125f * 19f;
        actualWidth = (tileWidth * mapSizeX);

        //float tileWidth = actualWidth / mapSizeX;   // Dimensions d'une tuile en unités de unity
        Vector3 spawnOffset = new Vector3(actualWidth / 2f, (tileWidth * mapSizeY) / -2f, 0f);
        spawnOffset.x -= 0.03125f;
        spawnOffset.y += 0.03125f;

        char startChar = 'A';
        startChar--;

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                // Instanciation des tuiles, à partir du array 2D de ints (voir fonction suivante)
                GameObject tilePrefab = GetTileFromID(_gameGrid[x, y]);
                Vector3 spawnPos = new Vector3(x * tileWidth, -y * tileWidth, 0f);
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
                //gt.InitializeTile(new Vector2(tileWidth, tileWidth), _currentGridMode);
                gt.tileName = GridCoords.GetTileName(new TileCoordinates(x, y));

                // Ajout de la tuile à l'array2D de tuiles
                _gameGridTiles[x, y] = gt;
            }
        }

        // Assignation des informations de la grille et appel de l'ACTION de nouvelle grille
        Bounds newBounds = new Bounds(Vector3.zero, new Vector3(actualWidth, tileWidth * mapSizeY, 0f));
        _currentGridInfo = new GridInfo(_gameGridTiles, new Vector2Int(mapSizeX, mapSizeY), newBounds);

        foreach (var tile in _gameGridTiles)
        {
            tile.InitializeTile(new Vector2(tileWidth, tileWidth), _currentGridMode, _currentGridInfo);
        }
    }

    private void GenerateEnvironment()
    {
        int prefabCount = tilemapPrefabs.Count;

        if (tilemapPrefabs.Count <= 0)
            return;

        int randomIndex = UnityEngine.Random.Range(0, prefabCount);
        AnomalyTileMapData anomalyTileMapData = Instantiate(tilemapPrefabs[randomIndex]).GetComponent<AnomalyTileMapData>();

        int[,] environmentData = anomalyTileMapData.GetTileData();
        int xLenght = environmentData.GetLength(0);
        int yLenght = environmentData.GetLength(1);
        int[,] randomizedXData = new int[xLenght, yLenght];
        int[,] randomizedYData = new int[xLenght, yLenght];

        // Randomize X
        int randomRoll = UnityEngine.Random.Range(0, 10);
        for (int y = 0; y < yLenght; y++)
        {
            for (int x = 0; x < xLenght; x++)
            {
                // Invert X
                if (randomRoll > 4)
                {
                    randomizedXData[xLenght - 1 - x, y] = environmentData[x, y];
                }
                // Don't invert X
                else
                {
                    randomizedXData[x, y] = environmentData[x, y];
                }
            }
        }

        // Randomize Y
        randomRoll = UnityEngine.Random.Range(0, 10);
        for (int x = 0; x < xLenght; x++)
        {
            for (int y = 0; y < yLenght; y++)
            {
                // Invert X
                if (randomRoll > 4)
                {
                    randomizedYData[x, yLenght - 1 -  y] = randomizedXData[x, y];
                }
                // Don't invert X
                else
                {
                    randomizedYData[x, y] = randomizedXData[x, y];
                }
            }
        }

        for (int x = 0; x < xLenght; x++)
        {
            if (x + 1 >= (int)_currentGridInfo.gameGridSize.x)
                break;

            for (int y = 0; y < yLenght; y++)
            {
                if (y + 1 >= (int)_currentGridInfo.gameGridSize.y)
                    break;

                if (randomizedYData[x,y] == 1)
                {
                    ReplaceTile(_currentGridInfo.gameGridTiles[x, y], 1);
                }
                else if (randomizedYData[x, y] == 4)
                {
                    ReplaceTile(_currentGridInfo.gameGridTiles[x, y], 5);
                }
            }
        }
        anomalyTileMapData.gameObject.SetActive(false);

        AssignAnomalyPatches();
    }

    private void AssignAnomalyPatches()
    {
        List<GridTile> foundTiles = new List<GridTile>();
        while (true)
        {
            int x = 0;
            int y = 0;

            GridTile newTileFound = null;
            for (x = 0; x < mapSizeX; x++)
            {
                bool found = false;
                for (y = 0; y < mapSizeY; y++)
                {
                    if (_currentGridInfo.gameGridTiles[x, y].tileType == 4 || _currentGridInfo.gameGridTiles[x, y].tileType == 5)
                    {
                        if (!foundTiles.Contains(_currentGridInfo.gameGridTiles[x, y]))
                        {
                            newTileFound = _currentGridInfo.gameGridTiles[x, y];
                            foundTiles.Add(newTileFound);
                            found = true;
                            break;
                        }
                    }
                }
                if (found)
                    break;
            }

            if (newTileFound != null)
            {
                List<GridTile> currentSegment = new List<GridTile>();
                currentSegment.Add(newTileFound);
                int countBeforeCheck = 0;

                while (currentSegment.Count != countBeforeCheck)
                {
                    countBeforeCheck = currentSegment.Count;
                    List<GridTile> newTiles = new List<GridTile>();

                    foreach (var tile in currentSegment)
                    {
                        List<GridTile> neighbours = tile.EightWayNeighbours;
                        foreach (var neighbour in neighbours)
                        {
                            if (neighbour != null)
                                if (neighbour.tileType == 4 || neighbour.tileType == 5)
                                {
                                    if (!currentSegment.Contains(neighbour))
                                        if (!newTiles.Contains(neighbour))
                                            newTiles.Add(neighbour);
                                }
                        }
                    }

                    foreach (var tile in newTiles)
                    {
                        currentSegment.Add(tile);
                    }
                }

                foreach (var tile in currentSegment)
                {
                    foundTiles.Add(tile);
                    Debug.DrawLine(tile.TileCenter, tile.TileCenter + Vector2.up * 0.5f, Color.blue, 5f);
                }

                _allAnomalyPatches.Add(new AnomalyPatch(currentSegment));
            }

            if (x >= _currentGridInfo.gameGridSize.x - 1 && y >= _currentGridInfo.gameGridSize.y)
                break;
        }
        Debug.Log("SEGMENT COUNT " + _allAnomalyPatches.Count);
    }

    private void GenerateDeployPoint()
    {
        // ------ DEPLOY POINTS -------
        List<GridTile> candidateTiles = new List<GridTile>();
        int candidatesCount = 0;

        GridQuadrants.QuadrantMatch quadrantMatch = _currentGridInfo.gameGridQuadrants.GetRandomQuadrantMatch(out _currentGridInfo.positiveQuadrantIndex);

        foreach (var tile in _currentGridInfo.gameGridTiles)
        {
            bool tileInQuadrant = false;

            Vector2 bottomLeft = tile.TileBounds.min;
            Vector2 topLeft = tile.TileBounds.min;
            topLeft.y = tile.TileBounds.max.y;
            Vector2 bottomRight = tile.TileBounds.max;
            bottomRight.y = tile.TileBounds.min.y;
            Vector2 topRight = tile.TileBounds.max;

            if (quadrantMatch.positiveQuadrant.Contains(bottomLeft))
                tileInQuadrant = true;
            if (quadrantMatch.positiveQuadrant.Contains(topLeft))
                tileInQuadrant = true;
            if (quadrantMatch.positiveQuadrant.Contains(bottomRight))
                tileInQuadrant = true;
            if (quadrantMatch.positiveQuadrant.Contains(topRight))
                tileInQuadrant = true;

            if (tileInQuadrant && tile.tileType == 0)
            {
                candidateTiles.Add(tile);
                candidatesCount++;
            }
        }

        if (candidatesCount > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidatesCount);

            TileCoordinates targetTile = candidateTiles[randomIndex].TileCoordinates;
            ReplaceTile(_currentGridInfo.gameGridTiles[targetTile.tileX, targetTile.tileY], 2);
        }
        else
        {
            Debug.Log("ERROR : No candidate tiles found for DeployTile");
        }
    }

    private void InstantiateAnomalySegment(TileCoordinates startTile)
    {
        //Debug.Log("X : " + startTile.tileX + "Y : " + startTile.tileY);
        GridTile tileToReplace = _gameGridTiles[startTile.tileX, startTile.tileY];
        GridTile newTile;
        ReplaceTile(tileToReplace, 10, out newTile);

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

        //Debug.Log("X : " + newAnomalyCoords.tileX + "Y : " + newAnomalyCoords.tileY);
        GridTile tileToReplace = _gameGridTiles[newAnomalyCoords.tileX, newAnomalyCoords.tileY];
        GridTile newTile;
        ReplaceTile(tileToReplace, 10, out newTile);

        AnomalySegment segment = new AnomalySegment();
        _allAnomalySegments.Add(segment);
        segment.AssignActiveTile(newTile);
    }

    // Fonction utilitaire qui donne le bon prefab de tuile, selon le int dans l'array2D
    private GameObject GetTileFromID(int tileID)
    {
        switch (tileID)
        {
            case 0: return emptyCellPrefab;
            case 1: return rocktilePrefab;
            case 2: return deployTilePrefab;
            case 3: return planetTilePrefab;
            case 4: return staticAnomalyPrefab;
            case 5: return spawningStaticAnomalyPrefab;
            case 10: return anomaly0Prefab;
            case 11: return anomaly1Prefab;
            case 12: return anomaly2Prefab;

            default: return emptyCellPrefab;
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
        gt.tileName = deadTile.tileName;
        gt.tileType = newTileType;
        gt.InitializeTile(deadTile.tileDimensions, _currentGridMode, _currentGridInfo);

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
        newTile.tileName = deadTile.tileName;
        newTile.PlanetHeat = deadTile.PlanetHeat;
        newTile.InitializeTile(deadTile.tileDimensions, _currentGridMode, _currentGridInfo);

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
        newTile.tileName = deadTile.tileName;
        newTile.tileType = newTileType;
        newTile.InitializeTile(deadTile.tileDimensions, _currentGridMode, _currentGridInfo);

        newTile.TransferObjectList(deadTile.CurrentObjectsInTile);

        // Remplacement de la tuile dans les arrays2D
        _gameGrid[deadTile.tileX, deadTile.tileY] = newTileType;
        _gameGridTiles[deadTile.tileX, deadTile.tileY] = newTile;
        deadTile.TriggerNeighbourTilesUpdates();
        deadTile.DisableTile();
    }

    // Fonction qui track la quantité de tuiles d'anomalie et qui trigger le spawn des prochains segments
    //public void OnNewAnomalyTileComplete(GridTile tile)
    //{
    //    _anomalyCompletedTileCount++;

    //    int anomalyStepCount = newSegmentAfterCount.Length;
    //    if (_allAnomalySegments.Count <= anomalyStepCount)
    //    {
    //        if (_anomalyCompletedTileCount == newSegmentAfterCount[_allAnomalySegments.Count - 1])
    //        {
    //            InstantiateAnomalySegment(_currentGridInfo.positiveQuadrantIndex);
    //        }
    //    }

    //    if (_anomalyCompletedTileCount >= _currentGridInfo.tileCount)
    //    {
    //        if (totalGridAnomaly != null)
    //            totalGridAnomaly();
    //    }
    //}

    // Fonction appelée par l'action dans la classe GridStaticObject
    // Permet d'ajouter un objet à la liste et ajouter à la tuile qui le contient
    private void OnGridObjectPositionAdded(GridStaticObject obj)
    {
        if (obj.ParentTile.tileX < 0 || obj.ParentTile.tileX >= _currentGridInfo.gameGridSize.x)
        {
            //Debug.Log("INVALID OBJECT X POSITION");
            return;
        }

        if (obj.ParentTile.tileY < 0 || obj.ParentTile.tileY >= _currentGridInfo.gameGridSize.y)
        {
            //Debug.Log("INVALID OBJECT Y POSITION");
            return;
        }

        // Ajout de l'objet à la liste 
        _allStaticObjects.Add(obj);

        // Ajout de l'objet aux données de la tuile qui le contient
        _gameGridTiles[obj.ParentTile.tileX, obj.ParentTile.tileY].AddObjectToTile(obj);
    }

    // Fonction DEBUG qui change une tuile au hasard par une anomalie
    public void SpawnRandomAnomalyTile()
    {
        int tileX = UnityEngine.Random.Range(0, mapSizeX);
        int tileY = UnityEngine.Random.Range(0, mapSizeY);

        ReplaceTile(_gameGridTiles[tileX, tileY], 10);
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

    public void ToggleGridMode()
    {
        switch (_currentGridMode)
        {
            case GridMode.WorldMap:
                _currentGridMode = GridMode.Debug;
                break;
            case GridMode.Debug:
                _currentGridMode = GridMode.WorldMap;
                break;
            default:
                break;
        }

        if (newGridMode != null)
            newGridMode(_currentGridMode);
    }

    private void OnSpawningAnomalyTileLifeOver(GridTile tile)
    {
        tile.tileLifeOver -= OnSpawningAnomalyTileLifeOver;
        GridTile_SpawningStaticAnomaly anomalyTile = tile.GetComponent<GridTile_SpawningStaticAnomaly>();
        AnomalyPatch parentPatch = anomalyTile.ParentPatch;
        int tileX = tile.tileX;
        int tileY = tile.tileY;

        ReplaceTile(tile, 4);
        parentPatch.AddTileToPatch(_currentGridInfo.gameGridTiles[tileX, tileY]);
    }

    private IEnumerator AnomalySpreadingTimer()
    {
        while (true)
        {
            if (!GameManager.GameStarted)
            {
                yield return new WaitForEndOfFrame();
                continue;
            }

            yield return new WaitForSeconds(anomalySpreadTime);

            int randomIndex = UnityEngine.Random.Range(0, _allAnomalyPatches.Count);
            AnomalyPatch targetPatch = _allAnomalyPatches[randomIndex];

            GridTile randomTile = targetPatch.GetRandomNewTile();

            if (randomTile == null)
                continue;

            int tileX = randomTile.tileX;
            int tileY = randomTile.tileY;

            ReplaceTile(randomTile, 5);

            GridTile newTile = _currentGridInfo.gameGridTiles[tileX, tileY];
            GridTile_SpawningStaticAnomaly anomalyTile = newTile.GetComponent<GridTile_SpawningStaticAnomaly>();
            anomalyTile.ParentPatch = targetPatch;

            newTile.tileLifeOver += OnSpawningAnomalyTileLifeOver;

        }
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

public enum GridMode
{
    WorldMap,
    Debug
}

