using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorManager : MonoBehaviour
{
    public static SectorManager instance;
    public static Action<SectorInfo> sectorInfoUpdate;

    private SectorInfo _currentSectorInfo;

    // TODO : Link to levelmanager
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

        OnNewSector();
    }

    private void OnNewSector()
    {
        _currentSectorInfo = new SectorInfo();
    }

    private void OnEnable()
    {
        Planet.soulsLost += OnSoulsLost;
        Planet.planetFullySaved += OnPlanetFullySaved;
        RessourceManager.ressourcesUpdate += OnRessourceUpdate;
    }

    private void OnDisable()
    {
        Planet.soulsLost -= OnSoulsLost;
        Planet.planetFullySaved -= OnPlanetFullySaved;
        RessourceManager.ressourcesUpdate -= OnRessourceUpdate;
    }

    private void OnRessourceUpdate(int soulsSaved, int soulBuffer, int currentCredits, int totalCreditsEarned)
    {
        _currentSectorInfo.SectorSoulsSaved = soulsSaved;
        _currentSectorInfo.CreditsGained = totalCreditsEarned;
        SendSectorInfoUpdates();
    }

    private void OnSoulsLost(Planet planet, int amount)
    {
        _currentSectorInfo.SectorSoulsLost += amount;
        SendSectorInfoUpdates();
    }

    private void OnPlanetFullySaved(Planet planet)
    {
        _currentSectorInfo.PlanetsFullySaved++;
        SendSectorInfoUpdates();
    }

    private void SendSectorInfoUpdates()
    {
        if (sectorInfoUpdate != null)
            sectorInfoUpdate(_currentSectorInfo);
    }
}
