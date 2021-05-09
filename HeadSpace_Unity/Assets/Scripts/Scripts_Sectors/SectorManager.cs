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

    private float startTime;

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
        _currentSectorInfo.DayIndex = GameManager.instance.CurrentDayInfo.day;
        SendSectorInfoUpdates();
    }

    private void OnEnable()
    {
        RessourceManager.ressourcesUpdate += OnRessourceUpdate;
        Contract.contractComplete += OnContractComplete;
        ContractManager.newContractReceived += OnNewContractReceived;
        TimeManager.timeStarted += OnTimeStart;
        TimeManager.levelTimerEnded += OnLevelEnd;
    }

    private void OnDisable()
    {
        RessourceManager.ressourcesUpdate -= OnRessourceUpdate;
        Contract.contractComplete -= OnContractComplete;
        ContractManager.newContractReceived -= OnNewContractReceived;
        TimeManager.timeStarted -= OnTimeStart;
        TimeManager.levelTimerEnded -= OnLevelEnd;
    }

    private void OnNewContractReceived()
    {
        _currentSectorInfo.TotalContracts++;
        SendSectorInfoUpdates();
    }

    private void OnContractComplete(int creditsGained, float time)
    {
        _currentSectorInfo.CreditsGained += creditsGained;
        _currentSectorInfo.totalContractTimes += time;
        _currentSectorInfo.ContractsCompleted++;

        if (creditsGained == 10)
            _currentSectorInfo.tenPointsContracts++;
        else if (creditsGained == 8)
            _currentSectorInfo.eightPointsContracts++;
        else if (creditsGained == 6)
            _currentSectorInfo.sixPointsContracts++;
        else if (creditsGained == 1)
            _currentSectorInfo.onePointsContracts++;

        SendSectorInfoUpdates();
    }

    private void OnTimeStart()
    {
        startTime = Time.time;
    }

    private void OnLevelEnd()
    {
        float gameDuration = Time.time - startTime;
        _currentSectorInfo.GameTime = gameDuration;
        SendSectorInfoUpdates();
    }

    private void OnRessourceUpdate(int creditsGained, int contractsCompleted)
    {
        //_currentSectorInfo.Cre = creditsGained;
        //_currentSectorInfo.ContractsCompleted = contractsCompleted;

        //if (creditsGained == 10)
        //    _currentSectorInfo.tenPointsContracts++;
        //else if (creditsGained == 8)
        //    _currentSectorInfo.eightPointsContracts++;
        //else if (creditsGained == 6)
        //    _currentSectorInfo.sixPointsContracts++;
        //else if (creditsGained == 1)
        //    _currentSectorInfo.onePointsContracts++;

        //SendSectorInfoUpdates();
    }

    private void SendSectorInfoUpdates()
    {
        if (sectorInfoUpdate != null)
            sectorInfoUpdate(_currentSectorInfo);
    }
}
