using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : GridStaticObject
{
    public static Action<Planet> newPlanetInSector;   // Action qui déclare la planète et envoie une référence lorsqu'elle apparait dans la scène
    public static Action<Planet, int> soulsLost;   // Action appelée à chaque PERTE de souls, envoie une ref de Planet et la qté de souls perdue

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

    private void Start()
    {
        // Lancer l'action et envoyer une référence vers son script Planet
        if (newPlanetInSector != null)
            newPlanetInSector(this);
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

        // Assigner et start la coroutine, s'il n'y en a pas déjà une en cours
        if (_currentDamageRoutine == null)
        {
            _currentDamageRoutine = DamageTick();
            StartCoroutine(_currentDamageRoutine);
        }

        // Arrêter la coroutine, si elle n'est pas null
        if (_currentDamageRoutine != null)
        {
            StopCoroutine(_currentDamageRoutine);
            _currentDamageRoutine = null;
        }
    }

    public int RemoveSoul(int soulAmount)
    {
        int soulsBeforeRemove = CurrentSouls;
        CurrentSouls = Mathf.Clamp(CurrentSouls - soulAmount, 0, TotalSouls);

        return soulsBeforeRemove - CurrentSouls;
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
            int previousSouls = CurrentSouls;
            CurrentSouls = Mathf.CeilToInt(_soulDamage);

            if (previousSouls < CurrentSouls)
            {
                int difference = CurrentSouls - previousSouls;

                // Appel de l'action qui envoie les infos sur la planète et la qté. d'habitants perdus
                if (soulsLost != null)
                    soulsLost(this, difference);
            }

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
