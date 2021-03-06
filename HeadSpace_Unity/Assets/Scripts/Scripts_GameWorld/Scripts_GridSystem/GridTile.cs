﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile : MonoBehaviour
{
    public static Action<GridTile> anomalyTileComplete;
    public Action<GridTile> tileLifeOver;

    // Références pour components
    protected SpriteRenderer _spriteRenderer;
    protected BoxCollider2D _boxCollider;
    protected Canvas _debugCanvas;
    public TextMeshProUGUI lifeTimeText;
    public TextMeshProUGUI heatText;

    // Temp
    public string tileName;

    // Propriétés des tuiles
    [Header("Life settings")]
    public bool liveForever;
    public float lifeTime;
    private float _lifeRemaining;
    public float LifeRemaining { get { return _lifeRemaining; } }

    [Header("Damage settings")]
    public float shipDPS;
    public float planetDPS;
    public float colliderEnableDelay;

    [Header("Sprites")]
    public Sprite worldMapSprite;
    public Sprite debugSprite;

    // Position sur la grille, assignee par le GridManager
    [HideInInspector] public int tileX;
    [HideInInspector] public int tileY;
    public TileCoordinates TileCoordinates {  get { return new TileCoordinates(tileX, tileY); } }
    public Bounds TileBounds { get { return _boxCollider.bounds; } }

    // Type de tuile en int, assigné par le GridManager
    public int tileType;
    [HideInInspector] public Vector2 tileDimensions;

    // Informations de la grille
    protected GridInfo _gridInfo;
    private GridMode _currentGridMode;
    // Liste d'objets statics qui se trouvent dans la tuile
    [SerializeField] private List<GridStaticObject> _currentObjectsInTile = new List<GridStaticObject>();
    // Propriété qui retourne une référence à la liste d'objets
    public List<GridStaticObject> CurrentObjectsInTile { get { return _currentObjectsInTile; } }

    // Voisins de la tuile
    [SerializeField] private GridTile[] _allNeighbours = new GridTile[4];
    public GridTile[] AllNeighbours { get { return _allNeighbours; } }
    [SerializeField] private GridTile[] _diagonalNeighbours = new GridTile[4];

    public int PlanetHeat { get; set; }
    public int ShipStartHeat { get; private set; }

    // PathFinding
    public PathNode PathNode { get; set; }

    public List<GridTile> EightWayNeighbours
    {
        get
        {
            List<GridTile> neighbours = new List<GridTile>();
            foreach (var nb in _allNeighbours)
            {
                if (nb != null)
                    neighbours.Add(nb);
            }
            foreach (var nb in _diagonalNeighbours)
            {
                if (nb != null)
                    neighbours.Add(nb);
            }
            //Debug.Log("UO");
            return neighbours;
        }
    }

    private List<GridTile> _emptyNeighbours = new List<GridTile>();
    public List<GridTile> EmptyNeighbours { get { return _emptyNeighbours; } }
    private List<GridTile> _anomalyNeighbours = new List<GridTile>();
    public List<GridTile> AnomalyNeighbours { get { return _anomalyNeighbours; } }

    public Vector2 TileCenter
    {
        get
        {
            return _boxCollider.bounds.center;
        }
    }

    // SUBSRIPTION à l'Action de nouvelle grille
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
        GridManager.newGridMode += ToggleGridMode;
    }

    // UNSUBSCRIPTION
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
        GridManager.newGridMode -= ToggleGridMode;
    }

    protected virtual void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _debugCanvas = GetComponentInChildren<Canvas>();
        _lifeRemaining = lifeTime;
        StartColliderDelay();
    }

    protected virtual void Start()
    {
        // SI la tuile n'a pas de durée de vie, cacher le debug text
        if (liveForever)
            lifeTimeText.enabled = false;

        // SINON, lancer le timer de durée de vie
        else
            StartCoroutine(LifeTimer());
    }

    // Assigner l'information de grille (appelée par l'action de nouvelle grille du GridManager)
    private void AssignGridInfo(GridInfo currentGridInfo)
    {
        _gridInfo = currentGridInfo;
        PathNode = new PathNode(currentGridInfo.gameGridTiles, this, tileX, tileY);
        UpdateNeighbours();
    }

    //Fonction pour initialiser les components de la tuile, lorsqu'elle spawn
    public virtual void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        this.tileDimensions = tileDimensions;

        _spriteRenderer.size = tileDimensions;
        _boxCollider.size = tileDimensions;
        Vector2 colliderOffset = new Vector2();
        colliderOffset = tileDimensions / 2f;
        colliderOffset.y = -colliderOffset.y;
        _boxCollider.offset = colliderOffset;
        _currentGridMode = gridMode;

        _debugCanvas.GetComponent<RectTransform>().sizeDelta = tileDimensions;

        ToggleGridMode(_currentGridMode);
        AssignCheckeredSprite();
    }

    // Fonction pour initialiser les components de la tuile, lorsqu'elle spawn
    public virtual void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        AssignGridInfo(currentGridInfo);
        InitializeTile(tileDimensions, gridMode);
    }

    protected virtual void ToggleGridMode(GridMode newGridMode)
    {
        switch (newGridMode)
        {
            case GridMode.WorldMap:
                _spriteRenderer.sprite = worldMapSprite;
                ToggleLifetimeText(false);
                ToggleHeatText(false);
                break;
            case GridMode.Debug:
                _spriteRenderer.sprite = debugSprite;
                ToggleLifetimeText(true);
                ToggleHeatText(true);
                break;
            default:
                break;
        }
    }

    // Fonction pour désactiver les components de la tuile
    public void DisableTile()
    {
        this.gameObject.SetActive(false);
        //_spriteRenderer.enabled = false;
        //_boxCollider.enabled = false;

        foreach (var obj in _currentObjectsInTile)
        {
            obj.gridObjectPositionRemoved -= RemoveObjectFromTile;
        }

        _currentObjectsInTile.Clear();
    }

    public void AddObjectToTile(GridStaticObject obj)
    {
        _currentObjectsInTile.Add(obj);
        obj.gridObjectPositionRemoved += RemoveObjectFromTile;
    }

    public void RemoveObjectFromTile(GridStaticObject obj)
    {
        _currentObjectsInTile.Remove(obj);
    }

    public void TransferObjectList(List<GridStaticObject> objList)
    {
        _currentObjectsInTile = new List<GridStaticObject>(objList);
        foreach (var obj in _currentObjectsInTile)
        {
            //Debug.Log("Found : " + obj.gameObject.name);
            obj.gridObjectPositionRemoved += RemoveObjectFromTile;
        }
    }

    public List<Planet> GetPlanetsInTile()
    {
        List<Planet> planetList = new List<Planet>();
        foreach (var obj in _currentObjectsInTile)
        {
            Planet candidate = obj.GetComponent<Planet>();
            if (candidate != null)
            {
                planetList.Add(candidate);
            }
        }
        return planetList;
    }

    // Fonction qui permet de vérifier si un point (Vector3) envoyé en paramètre se trouve à L'INTÉRIEUR du cercle
    public bool IsInTile(Vector2 targetPoint)
    {
        return _boxCollider.bounds.Contains(targetPoint);
    }

    private IEnumerator LifeTimer()
    {
        float seconds = lifeTime;
        UpdateDebugText(seconds);

        while (!GameManager.GameStarted)
        {
            yield return new WaitForEndOfFrame();
        }

        while (seconds > 0.01f)
        {
            yield return new WaitForSeconds(0.1f);
            seconds -= 0.1f;
            _lifeRemaining = seconds;
            UpdateDebugText(seconds);
        }

        LifeTimerOver();
    }

    protected virtual void LifeTimerOver()
    {
        if (!liveForever)
        {
            if (tileLifeOver != null)
                tileLifeOver(this);
        }

        //// IF ANOMALY TILE IS COMPLETING
        //if (tileType == 2)
        //{
        //    if (anomalyTileComplete != null)
        //        anomalyTileComplete(this);
        //}

        ToggleLifetimeText(false);
    }

    private IEnumerator StartColliderDelay()
    {
        _boxCollider.enabled = false;
        yield return new WaitForSeconds(colliderEnableDelay);
        _boxCollider.enabled = true;
    }

    public virtual void UpdateNeighbours()
    {
        _anomalyNeighbours.Clear();
        _emptyNeighbours.Clear();

        int maxIndexX = _gridInfo.gameGridTiles.GetLength(0) - 1;
        int maxIndexY = _gridInfo.gameGridTiles.GetLength(1) - 1;

        // Top Neighbour
        if (tileY + 1 <= maxIndexY)
        {
            _allNeighbours[0] = _gridInfo.gameGridTiles[tileX, tileY + 1];

            // TopLeft diagonal neighbour
            if (tileX - 1 >= 0)
            {
                _diagonalNeighbours[0] = _gridInfo.gameGridTiles[tileX - 1, tileY + 1];
            }

            // TopRight diagonal neighbour
            if (tileX + 1 <= maxIndexX)
            {
                _diagonalNeighbours[1] = _gridInfo.gameGridTiles[tileX + 1, tileY + 1];
            }
        }

        // Bottom Neighbour
        if (tileY - 1 >= 0)
        {
            _allNeighbours[1] = _gridInfo.gameGridTiles[tileX, tileY - 1];

            // TopLeft diagonal neighbour
            if (tileX - 1 >= 0)
            {
                _diagonalNeighbours[2] = _gridInfo.gameGridTiles[tileX - 1, tileY - 1];
            }

            // TopRight diagonal neighbour
            if (tileX + 1 <= maxIndexX)
            {
                _diagonalNeighbours[3] = _gridInfo.gameGridTiles[tileX + 1, tileY - 1];
            }
        }
            

        // Left Neighbour
        if (tileX - 1 >= 0)
            _allNeighbours[2] = _gridInfo.gameGridTiles[tileX - 1, tileY];

        // Right Neighbour
        if (tileX + 1 <= maxIndexX)
            _allNeighbours[3] = _gridInfo.gameGridTiles[tileX + 1, tileY];

        // Assigner les Neighbours à la bonne liste, selon leur Type
        for (int i = 0; i < _allNeighbours.Length; i++)
        {
            if (_allNeighbours[i] != null)
            {
                // 0 = EMPTY TILE
                if (_allNeighbours[i].tileType == 0)
                {
                    _emptyNeighbours.Add(_allNeighbours[i]);
                }
                // > 0 = ANOMALY
                else
                {
                    _anomalyNeighbours.Add(_allNeighbours[i]);
                }
            }
        }
    }

    public void TriggerNeighbourTilesUpdates()
    {
        for (int i = 0; i < _allNeighbours.Length; i++)
        {
            if (_allNeighbours[i] != null)
            {
                _allNeighbours[i].UpdateNeighbours();
            }
        }
    }

    private void ToggleLifetimeText(bool toggleON)
    {
        if (liveForever)
        {
            lifeTimeText.enabled = false;
            return;
        }
        lifeTimeText.enabled = toggleON;
    }

    private void ToggleHeatText(bool toggleON)
    {
        if (heatText != null)
            heatText.enabled = toggleON;
    }

    private void UpdateHeatText()
    {
        if (heatText != null)
            heatText.text = PlanetHeat.ToString();
    }

    private void UpdateDebugText(float time)
    {
        if (lifeTimeText == null)
            return;

        lifeTimeText.text = TimeSpan.FromSeconds(time).ToString(@"m\:ss");
    }

    public void AddPlanetHeat(int amount)
    {
        PlanetHeat += amount;
        UpdateHeatText();
    }

    public void AddShipHeat(int amount)
    {
        ShipStartHeat += amount;
    }

    protected virtual void AssignCheckeredSprite()
    {
        bool even = (tileX + tileY) % 2 == 0 ? true : false;
        if (even)
            _spriteRenderer.sprite = _gridInfo.evenSprite;
        else
            _spriteRenderer.sprite = _gridInfo.oddSprite;

    }
}

