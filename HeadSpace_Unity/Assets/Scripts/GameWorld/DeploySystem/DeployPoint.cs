using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployPoint : MonoBehaviour
{
    public static Action<DeployPoint> newDeployPoint;

    // Components
    private CircleCollider2D _collider;
    public float radius;

    private void Awake()
    {
        // Assigner les références de components
        _collider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // TEMP : Size hardcoded
        this.transform.localScale = Vector3.one * radius;
        if (newDeployPoint != null)
            newDeployPoint(this);
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
}
