using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script qui envoie un signal au script PLANET, lorsqu'un objet entre dans le trigger de sa ZONE DE DÉTECTION
public class Planet_DetectionZone : MonoBehaviour
{
    // Référence à la planète à laquelle la zone appartient
    private Planet _parentPlanet;

    private void Awake()
    {
        // Assigner la référence
        _parentPlanet = GetComponentInParent<Planet>();
    }

    // Fonction qui envoie tout simplement le signal de collision, ainsi que le collider à sa planète parent
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_parentPlanet != null)
        {
            _parentPlanet.OnDetectionZoneTrigger(collider);
        }
    }
}
