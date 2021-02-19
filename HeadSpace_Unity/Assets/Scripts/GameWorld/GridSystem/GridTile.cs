using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile : MonoBehaviour
{
    public static Action<GridTile> tileLifeOver;

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

    // Liste d'objets statics qui se trouvent dans la tuile
    [SerializeField] private List<GridStaticObject> _currentObjectsInTile = new List<GridStaticObject>();
    // Propriété qui retourne une référence à la liste d'objets
    public List<GridStaticObject> CurrentObjectsInTile { get { return _currentObjectsInTile; } }

    private void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _lifeTimeText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        // SI la tuile n'a pas de durée de vie, cacher le debug text
        if (liveForever)
            _lifeTimeText.enabled = false;

        // SINON, lancer le timer de durée de vie
        else
            StartCoroutine(LifeTimer());
    }

    // Fonction pour initialiser les components de la tuile, lorsqu'elle spawn
    public void InitializeTile(Vector2 tileDimensions)
    {
        this.tileDimensions = tileDimensions;

        _spriteRenderer.size = tileDimensions;
        _boxCollider.size = tileDimensions;
        _boxCollider.offset = tileDimensions / 2f;
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
            Debug.Log("Found : " + obj.gameObject.name);
            obj.gridObjectPositionRemoved += RemoveObjectFromTile;
        }
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
    }


    private void UpdateDebugText(float time)
    {
        if (_lifeTimeText == null)
            return;

        _lifeTimeText.text = TimeSpan.FromSeconds(time).ToString(@"m\:ss");
    }

}
