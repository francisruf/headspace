using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GridStaticObject
{
    public float damageStartBuffer;

    // Envoi de notification d'urgence (quand x % est perdu)
    public int distressNotificationPercent;
    private bool distressSent;

    // Propriétés de la planète
    public int TotalSouls { get; private set; }
    public int CurrentSouls { get; private set; }
    public int CompletionCreditsBonus { get; private set; }
    public string ArchetypeName { get; private set; }

    // Variables qui track la perte d'habitants en temps réel
    private float _soulDamage;
    private float _currentDPS;
    private float _currentAnomalyTileLife;

    private IEnumerator _currentDamageRoutine;

    // Référence au collider d'anomaly pour aller chercher DPS
    private Planet_AnomalyZone _anomalyZone;

    protected override void Awake()
    {
        base.Awake();
        _anomalyZone = GetComponentInChildren<Planet_AnomalyZone>();
    }

    public void AssignArchetype(PlanetArchetype archetype)
    {
        ArchetypeName = archetype.archetypeName;
        TotalSouls = archetype.GetRandomPopulation();
        CurrentSouls = TotalSouls;
        CompletionCreditsBonus = archetype.completionCreditsBonus;
        this.gameObject.name = "Planet_" + ArchetypeName;
    }

    // Fonction appellée par le script InteractionZone, sur son CHILD object
    // La logique est gérée entièrement ici, et non dans le script InteractionZone
    public void OnInteractionZoneTrigger(Collider2D collider)
    {
        //Debug.Log(collider.gameObject.name + " has entered my interaction zone.");
    }

    // Fonction appellée par le script DetectionZone, sur son CHILD object
    // La logique est gérée entièrement ici, et non dans le script DetectionZone
    public void OnDetectionZoneTrigger(Collider2D collider)
    {
        //Debug.Log(collider.gameObject.name + " has entered my detection zone.");
    }

    public void ToggleSprite()
    {
        _spriteRenderer.enabled = !_spriteRenderer.enabled;
    }

    public void NewDPSSettings(float dps, float anomalyTileLife)
    {
        _currentDPS = dps;

        if (anomalyTileLife < 9999f)
        {
            _currentAnomalyTileLife = anomalyTileLife;
        }

        if (_currentDamageRoutine == null)
        {
            _currentDamageRoutine = DamageTick();
            StartCoroutine(_currentDamageRoutine);
        }
    }

    private IEnumerator DamageTick()
    {
        yield return new WaitForSeconds(damageStartBuffer);

        _soulDamage = TotalSouls;
        float soulTicks = (0.75f * TotalSouls) / (_currentAnomalyTileLife - damageStartBuffer);

        Debug.Log("HELP, WE ARE IN DANGER!");

        while (CurrentSouls > 0)
        {
            yield return new WaitForSeconds(1f);
            _soulDamage -= soulTicks * _currentDPS;
            CurrentSouls = Mathf.CeilToInt(_soulDamage);

            if (((float)CurrentSouls / TotalSouls) < distressNotificationPercent / 100f && !distressSent)
            {
                Debug.Log("WE ARE DYING");
                distressSent = true;
            }
        }

        CurrentSouls = 0;
        Debug.Log("PLANET LOST.");
        _currentDamageRoutine = null;
    }
}
