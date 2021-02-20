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
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;

    // Propriété publique : Coordonnées de l'objet sur la grille de jeu (ET NON EN WORLD UNITS / UNITY UNITS)
    public Vector2 GridCoordinates { get; private set; }
    public TileCoordinates ParentTile { get; private set; }

    private void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
    }

    // Fonction qui permet d'assigner coordonnées de grille et placer l'objet dans le monde en conséquence
    public void PlaceGridObject(Vector2 gridCoordinates)
    {
        GridCoordinates = gridCoordinates;
        ParentTile = GridCoords.FromGridToTile(gridCoordinates);
        this.transform.position = GridCoords.FromGridToWorld(gridCoordinates);

        if (gridObjectPositionAdded != null)
            gridObjectPositionAdded(this);
    }

    // Désactiver un objet et ses components
    public void DisableGridObject()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;

        if (gridObjectPositionRemoved != null)
            gridObjectPositionRemoved(this);
    }
}
