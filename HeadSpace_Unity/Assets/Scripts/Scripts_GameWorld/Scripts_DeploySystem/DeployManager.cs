using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployManager : MonoBehaviour
{
    // Singleton
    public static DeployManager instance;

    // Liste de tous les points de déploiement sur la map
    private List<DeployPoint> _allDeployPoints = new List<DeployPoint>();
    private List<GridTile_DeployPoint> _allDeployTiles = new List<GridTile_DeployPoint>();

    // Subscription aux Actions
    private void OnEnable()
    {
        DeployPoint.newDeployPoint += OnNewDeployPoint;
        GridTile_DeployPoint.newDeployTile += OnNewDeployTile;
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
    }

    // Unsubscription
    private void OnDisable()
    {
        DeployPoint.newDeployPoint -= OnNewDeployPoint;
        GridTile_DeployPoint.newDeployTile += OnNewDeployTile;
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
    }

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

    // Ajouter les points de déploiement à la liste lorsqu'ils sont activés
    private void OnNewDeployPoint(DeployPoint deployPoint)
    {
        _allDeployPoints.Add(deployPoint);
    }

    private void OnNewDeployTile(GridTile_DeployPoint deployTile)
    {
        _allDeployTiles.Add(deployTile);
    }

    public List<GridTile_DeployPoint> GetDeployTiles()
    {
        return _allDeployTiles;
    }

    // Vider la liste lorsque la grille est détruite
    private void OnGridDataDestroyed()
    {
        _allDeployPoints.Clear();
        _allDeployTiles.Clear();
    }

    public bool IsInDeployTile(TileCoordinates tileCoords)
    {
        Vector2 worldCoords = GridCoords.FromTilePositionToWorld(tileCoords);

        foreach (var deployTile in _allDeployTiles)
        {
            if (deployTile.IsInTile(worldCoords))
                return true;
        }
        return false;
    }

    // Fonction qui permet de vérifier si une coordonnée donnée se trouve dans un point de déploiement
    public bool IsInDeployPoint(Vector2 gridCoords)
    {
        Vector2 worldCoords = GridCoords.FromGridToWorld(gridCoords);

        foreach (var deployPoint in _allDeployPoints)
        {
            if (deployPoint.IsInRadius(worldCoords))
                return true;
        }

        return false;
    }

    public List<GridTile_DeployPoint> GetAllDeployTiles()
    {
        return _allDeployTiles;
    }

    public List<GridTile> GetAllDeployTouchingTiles()
    {
        List<GridTile> allTouchingTiles = new List<GridTile>();

        foreach (var point in _allDeployPoints)
        {
            List<GridTile> temp = point.GetTouchingTiles();
            foreach (var tile in temp)
            {
                allTouchingTiles.Add(tile);
            }
        }
        return allTouchingTiles;
    }
}
