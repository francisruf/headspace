using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployManager : MonoBehaviour
{
    // Singleton
    public static DeployManager instance;

    // Liste de tous les points de déploiement sur la map
    private List<DeployPoint> _allDeployPoints = new List<DeployPoint>();

    // Subscription aux Actions
    private void OnEnable()
    {
        DeployPoint.newDeployPoint += OnNewDeployPoint;
    }

    // Unsubscription
    private void OnDisable()
    {
        DeployPoint.newDeployPoint -= OnNewDeployPoint;
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


}
