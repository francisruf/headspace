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

    [Header("Tile prefabs")]
    // Prefabs de tuiles et objets
    public GameObject emptyCellPrefab;
    public GameObject anomaly0Prefab;
    public GameObject anomaly1Prefab;
    public GameObject anomaly2Prefab;

    [Header("Object prefabs")]
    public GameObject deployPointPrefab;

    [Header("Deploy point settings")]
    public Vector2 deployStartCoordinates;

    // La grille
    private int[,] _gameGrid;   // Array2D d'ints pour génération initiale et pathfinding
    private GridTile[,] _gameGridTiles;    // Array 2D pour la grille de TUILES physiques

    // Informations de la grille actuelle (voir Struct en bas de cette classe)
    private GridInfo _currentGridInfo;

    // Subscription aux ACTIONS d'autres classes
    private void OnEnable()
    {
        GridTile.tileLifeOver += OnTileLifeOver;
        GridStaticObject.gridObjectPositionAdded += OnGridObjectPositionAdded;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridTile.tileLifeOver -= OnTileLifeOver;
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
            GenerateStartingObjects();
        }
    }

    // Fonction qui détruit l'ancienne grille, si elle existe
    private void DestroyMapData()
    {
        foreach (var gridTile in _gameGridTiles)
        {
            Destroy(gridTile.gameObject);
        }
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

        if (newGameGrid != null)
            newGameGrid(_currentGridInfo);
    }

    // Fonction qui génère les premiers objets sur la grille (points de déploiement / planètes(TBD))
    private void GenerateStartingObjects()
    {
        GameObject go = Instantiate(deployPointPrefab);

        // TEMP Instantiation sous le DebugManager
        if (DebugManager.instance != null)
        {
            go.transform.SetParent(DebugManager.instance.gridDebug.transform);
        }

        GridStaticObject obj = go.GetComponent<GridStaticObject>();
        obj.PlaceGridObject(deployStartCoordinates);
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

    // Fonction qui gère le passage d'une tuile à une autre, quand son temps de vie est écoulé.
    private void OnTileLifeOver(GridTile deadTile)
    {
        switch (deadTile.tileType)
        {
            case 0:
                {
                    break;
                }

            case 1:
                {
                    ReplaceTile(deadTile, 2);
                    break;
                }

            case 2:
                {
                    ReplaceTile(deadTile, 3);
                    break;
                }

            case 3:
                {
                    break;
                }

            default:
                break;
        }
    }

    // Fonction qui remplace une tuile existante par un nouveau type de tuile
    private void ReplaceTile(GridTile deadTile, int newTileType)
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
        gt.InitializeTile(deadTile.tileDimensions);

        gt.TransferObjectList(deadTile.CurrentObjectsInTile);

        // Remplacement de la tuile dans les arrays2D
        _gameGrid[deadTile.tileX, deadTile.tileY] = newTileType;
        _gameGridTiles[deadTile.tileX, deadTile.tileY] = gt;

        deadTile.DisableTile();
    }

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

        _gameGridTiles[obj.ParentTile.tileX, obj.ParentTile.tileY].AddObjectToTile(obj);
    }

    // Fonction DEBUG qui change une tuile au hasard par une anomalie
    public void SpawnRandomAnomalyTile()
    {
        int tileX = UnityEngine.Random.Range(0, mapSizeX);
        int tileY = UnityEngine.Random.Range(0, mapSizeY);

        ReplaceTile(_gameGridTiles[tileX, tileY], 1);
    }
}

/* STRUCT : GridInfo
 * - Un struct est une STRUCTURE de données semblable à une classe.
 * - Ce struct permet de mémoriser plusieurs variables dans un même contenant.
 * -- Dans ce cas, les informations de la grille actuelle.
 */

public struct GridInfo
{
    // Variables
    public GridTile[,] gameGridTiles;
    public Vector2 gameGridSize;
    public Bounds gameGridWorldBounds;

    // CONSTRUCTEUR : Fonction qui sert à déclarer un nouvel objet de type GridInfo (avec new GridInfo(...))
    // - Et d'assigner les valeurs des variables, à l'aide des paramètres en appel (dans les parenthèses)
    public GridInfo(GridTile[,] gameGridTiles, Vector2 gameGridSize, Bounds gameGridWorldBounds)
    {
        this.gameGridTiles = gameGridTiles;
        this.gameGridSize = gameGridSize;
        this.gameGridWorldBounds = gameGridWorldBounds;
    }


    // Complicated stuff
    public static bool operator ==(GridInfo op1, GridInfo op2)
    {
        return op1.Equals(op2);
    }

    public static bool operator !=(GridInfo op1, GridInfo op2)
    {
        return !op1.Equals(op2);
    }
}

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

