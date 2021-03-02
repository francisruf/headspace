using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script qui détecte les collisions avec l'anomalie, et définit le DPS effectué à la planète (implémenté dans le script PLANET)
public class Planet_AnomalyZone : MonoBehaviour
{
    // Référence au script Planet
    private Planet _parentPlanet;

    // Liste de toutes les cases d'anomalie qui touchent à la planète
    private List<GridTile_Anomaly> _allAnomalyTiles = new List<GridTile_Anomaly>();

    // Damage actuel
    private float _currentDPS;
    private float _tileLife;

    private void Awake()
    {
        _parentPlanet = GetComponentInParent<Planet>();
    }

    private void Update()
    {
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        GridTile_Anomaly tile = collider.GetComponent<GridTile_Anomaly>();

        if (tile != null)
        {
            _allAnomalyTiles.Add(tile);

            if (tile.planetDPS > _currentDPS)
            {
                _currentDPS = tile.planetDPS;
                _tileLife = tile.lifeTime;

                _parentPlanet.NewDPSSettings(_currentDPS, _tileLife);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collider)
    {
        GridTile_Anomaly tile = collider.GetComponent<GridTile_Anomaly>();

        if (tile != null)
        {
            if (_allAnomalyTiles.Contains(tile))
            {
                _allAnomalyTiles.Remove(tile);
            }
        }
    }
}
