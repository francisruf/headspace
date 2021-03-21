using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployPoint : GridStaticObject
{
    public static Action<DeployPoint> newDeployPoint;

    // Paramètres
    public float radius;

    // LayerMask pour vérifier les tiles qui touchent au DeployPoint
    public LayerMask touchingTilesLayerMask;

    protected override void OnEnable()
    {
        base.OnEnable();
        // TEMP : Size hardcoded
        this.transform.localScale = Vector3.one * radius;
        if (newDeployPoint != null)
            newDeployPoint(this);
    }

    protected override void Start()
    {
        
    }

    public override void PlaceGridObject(Vector2 gridCoordinates)
    {
        GridCoordinates = gridCoordinates;
        ParentTile = GridCoords.FromGridToTile(gridCoordinates);
        this.transform.position = GridCoords.FromGridToWorld(gridCoordinates);

        if (gridObjectPositionAdded != null)
            gridObjectPositionAdded(this);
    }

    // Fonction qui permet de vérifier si un point (Vector3) envoyé en paramètre se trouve à L'INTÉRIEUR du cercle
    public bool IsInRadius(Vector2 targetPoint)
    {
        // Si la distance du point au centre est plus petite que le radius (rayon) du cercle, le point se trouve dans le cercle
        float centerDistance = Vector2.Distance(transform.position, targetPoint);

        if (centerDistance <= (radius + 0.001f))   // Petite marge d'erreur
        {
            return true;
        }
        return false;
    }

    public List<GridTile> GetTouchingTiles()
    {
        List<GridTile> touchingTiles = new List<GridTile>();
        Collider2D[] allTileColliders = Physics2D.OverlapCircleAll(transform.position, radius, touchingTilesLayerMask);

        foreach (var tileCol in allTileColliders)
        {
            GridTile candidate = tileCol.GetComponent<GridTile>();

            if (candidate != null)
            {
                touchingTiles.Add(candidate);
                Debug.DrawRay(candidate.transform.position, Vector2.up, Color.cyan, 5f);
            }
        }
        //Debug.Log(gameObject.name + " says : " + touchingTiles.Count + " tiles touching");
        return touchingTiles;
    }
}
