using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridStaticObject : MonoBehaviour
{
    // ACTIONS / EVENTS appelés à chaque nouvelle position d'un objet, qu'il soit ajouté ou enlevé
    public static Action<GridStaticObject> gridObjectPositionAdded;
    public Action<GridStaticObject> gridObjectPositionRemoved;

    // Components
    protected SpriteRenderer _spriteRenderer;
    protected Collider2D _collider;

    public string objectNameLine;
    
    // Propriété publique : Coordonnées de l'objet sur la grille de jeu (ET NON EN WORLD UNITS / UNITY UNITS)
    public Vector2 GridCoordinates { get; protected set; }
    public TileCoordinates ParentTile { get; protected set; }

    protected virtual void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    protected virtual void OnEnable()
    {
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
    }

    protected virtual void OnDisable()
    {
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
    }

    // Fonction qui permet d'assigner coordonnées de grille et placer l'objet dans le monde en conséquence
    public virtual void PlaceGridObject(Vector2 gridCoordinates)
    {
        GridCoordinates = gridCoordinates;
        ParentTile = GridCoords.FromGridToTile(gridCoordinates);
        this.transform.position = GridCoords.FromGridToWorld(gridCoordinates);

        if (gridObjectPositionAdded != null)
            gridObjectPositionAdded(this);
    }

    // Désactiver un objet et ses components
    public virtual void DisableGridObject()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;

        if (gridObjectPositionRemoved != null)
            gridObjectPositionRemoved(this);
    }

    public virtual void ToggleSprite()
    {
        _spriteRenderer.enabled = !_spriteRenderer.enabled;
    }

    private void OnGridDataDestroyed()
    {
        Destroy(this.gameObject);
    }
}
