using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile : MonoBehaviour
{
    public static Action<GridTile> anomalyTileComplete;
    public Action<GridTile> tileLifeOver;

    // Références pour components
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;
    private TextMeshProUGUI _lifeTimeText;

    // Propriétés des tuiles
    [Header("Life settings")]
    public bool liveForever;
    public float lifeTime;

    [Header("Damage settings")]
    public float shipDPS;
    public float planetDPS;

    // Position sur la grille, assignee par le GridManager
    [HideInInspector] public int tileX;
    [HideInInspector] public int tileY;

    // Type de tuile en int, assigné par le GridManager
    [HideInInspector] public int tileType;
    [HideInInspector] public Vector2 tileDimensions;

    // Informations de la grille
    private GridInfo _gridInfo;
    // Liste d'objets statics qui se trouvent dans la tuile
    [SerializeField] private List<GridStaticObject> _currentObjectsInTile = new List<GridStaticObject>();
    // Propriété qui retourne une référence à la liste d'objets
    public List<GridStaticObject> CurrentObjectsInTile { get { return _currentObjectsInTile; } }

    // Voisins de la tuile
    [SerializeField] private GridTile[] _allNeighbours = new GridTile[4];

    private List<GridTile> _emptyNeighbours = new List<GridTile>();
    public List<GridTile> EmptyNeighbours { get { return _emptyNeighbours; } }
    private List<GridTile> _anomalyNeighbours = new List<GridTile>();
    public List<GridTile> AnomalyNeighbours { get { return _anomalyNeighbours; } }

    // SUBSRIPTION à l'Action de nouvelle grille
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
    }

    // UNSUBSCRIPTION
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
    }

    private void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _lifeTimeText = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected virtual void Start()
    {
        // SI la tuile n'a pas de durée de vie, cacher le debug text
        if (liveForever)
            _lifeTimeText.enabled = false;

        // SINON, lancer le timer de durée de vie
        else
            StartCoroutine(LifeTimer());
    }

    // Assigner l'information de grille (appelée par l'action de nouvelle grille du GridManager)
    private void AssignGridInfo(GridInfo currentGridInfo)
    {
        _gridInfo = currentGridInfo;
        UpdateNeighbours();
    }

    // Fonction pour initialiser les components de la tuile, lorsqu'elle spawn
    public void InitializeTile(Vector2 tileDimensions)
    {
        this.tileDimensions = tileDimensions;

        _spriteRenderer.size = tileDimensions;
        _boxCollider.size = tileDimensions;
        _boxCollider.offset = tileDimensions / 2f;
    }

    // Fonction pour initialiser les components de la tuile, lorsqu'elle spawn
    public void InitializeTile(Vector2 tileDimensions, GridInfo currentGridInfo)
    {
        this.tileDimensions = tileDimensions;

        _spriteRenderer.size = tileDimensions;
        _boxCollider.size = tileDimensions;
        _boxCollider.offset = tileDimensions / 2f;

        AssignGridInfo(currentGridInfo);
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

    private IEnumerator LifeTimer()
    {
        float seconds = lifeTime;
        UpdateDebugText(seconds);

        while (seconds > 0.01f)
        {
            yield return new WaitForSeconds(1f);
            seconds--;
            UpdateDebugText(seconds);
        }

        _lifeTimeText.enabled = false;

        if (!liveForever)
        {
            if (tileLifeOver != null)
                tileLifeOver(this);
        }

        // IF ANOMALY TILE IS COMPLETING
        if (tileType == 2)
        {
            if (anomalyTileComplete != null)
                anomalyTileComplete(this);
        }
    }

    public virtual void UpdateNeighbours()
    {
        _anomalyNeighbours.Clear();
        _emptyNeighbours.Clear();

        int maxIndexX = _gridInfo.gameGridTiles.GetLength(0) - 1;
        int maxIndexY = _gridInfo.gameGridTiles.GetLength(1) - 1;

        // Top Neighbour
        if (tileY + 1 <= maxIndexY)
            _allNeighbours[0] = _gridInfo.gameGridTiles[tileX, tileY + 1];

        // Bottom Neighbour
        if (tileY - 1 >= 0)
            _allNeighbours[1] = _gridInfo.gameGridTiles[tileX, tileY - 1];

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

    private void UpdateDebugText(float time)
    {
        if (_lifeTimeText == null)
            return;

        _lifeTimeText.text = TimeSpan.FromSeconds(time).ToString(@"m\:ss");
    }

}
