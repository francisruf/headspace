﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
    public static Action<int, int> ressourcesUpdate;
    public static Action<int> creditsUpdate;
    public static RessourceManager instance;

    // TOTAL RESSOURCES
    public int TotalCredits { get; private set; }
    public int TotalContractsCompleted { get; private set; }

    // LOCAL SECTOR RESSOURCES
    private int _contractsCompleted;
    private int _currentCredits;
    public int CurrentCredits { get { return _currentCredits; } }

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

    private void OnEnable()
    {
        //Ship.soulsUnloaded += OnShipUnload;
        Contract.contractComplete += OnContractComplete;
    }

    private void OnDisable()
    {
        //Ship.soulsUnloaded -= OnShipUnload;
        Contract.contractComplete -= OnContractComplete;
    }

    private void Start()
    {
        UpdateRessources();
    }

    private void OnShipUnload(int soulsUnloaded)
    {
        AddSouls(soulsUnloaded);
    }

    private void AddSouls(int newSouls)
    {
        UpdateRessources();
    }

    private void OnContractComplete(int reward)
    {
        _contractsCompleted++;
        TotalContractsCompleted++;
        AddCredits(reward);
    }

    public void AddCredits(int amount, bool dontAddToTotal = false)
    {
        _currentCredits += amount;
        TotalCredits += amount;
        UpdateRessources();
    }

    public bool SpendCredits(int amount)
    {
        if (_currentCredits >= amount)
        {
            _currentCredits -= amount;
            TotalCredits -= amount;
            UpdateRessources();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateRessources()
    {
        if (ressourcesUpdate != null)
            ressourcesUpdate(_currentCredits, _contractsCompleted);

        if (creditsUpdate != null)
            creditsUpdate(_currentCredits);
    }
}
