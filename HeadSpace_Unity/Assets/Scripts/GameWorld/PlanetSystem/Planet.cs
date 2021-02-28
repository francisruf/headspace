using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GridStaticObject
{
    // Propriétés de la planète
    public int totalSouls;
    public int currentSouls;
    public int completionCreditsBonus;

    void Start()
    {
        currentSouls = totalSouls;
    }

    // Fonction appellée par le script InteractionZone, sur son CHILD object
    // La logique est gérée entièrement ici, et non dans le script InteractionZone
    public void OnInteractionZoneTrigger(Collider2D collider)
    {
        Debug.Log(collider.gameObject.name + " has entered my interaction zone.");
    }

    // Fonction appellée par le script DetectionZone, sur son CHILD object
    // La logique est gérée entièrement ici, et non dans le script DetectionZone
    public void OnDetectionZoneTrigger(Collider2D collider)
    {
        Debug.Log(collider.gameObject.name + " has entered my detection zone.");
    }
}
