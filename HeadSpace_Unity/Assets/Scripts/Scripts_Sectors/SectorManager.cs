using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorManager : MonoBehaviour
{
    public static SectorManager instance;
    public static Action<SectorInfo> sectorInfoUpdate;

    private SectorInfo _currentSectorInfo;
    public SectorInfo CurrentSectorInfo { get { return _currentSectorInfo; } }

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
        RessourceManager.ressourcesUpdate += OnRessourceUpdate;
    }

    private void OnDisable()
    {
        RessourceManager.ressourcesUpdate -= OnRessourceUpdate;
    }

    private void OnRessourceUpdate(int creditsGained, int contractsCompleted)
    {
        _currentSectorInfo.CreditsGained = creditsGained;
        _currentSectorInfo.ContractsCompleted = contractsCompleted;
        SendSectorInfoUpdates();
    }

    private void SendSectorInfoUpdates()
    {
        if (sectorInfoUpdate != null)
            sectorInfoUpdate(_currentSectorInfo);
    }
}
