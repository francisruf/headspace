using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceManager : MonoBehaviour
{
    public static Action<int, int, int, int> ressourcesUpdate;
    public static Action<int> creditsUpdate;

    public static RessourceManager instance;

    public int soulRatioForOneCredit;

    private int _soulsSaved;
    private int _soulBuffer;
    private int _currentCredits;
    public int CurrentCredits { get { return _currentCredits; } }
    private int _totalCreditsEarned;

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
        Ship.soulsUnloaded += OnShipUnload;
    }

    private void OnDisable()
    {
        Ship.soulsUnloaded -= OnShipUnload;
    }

    private void Start()
    {
        UpdateRessources();

        if (soulRatioForOneCredit == 0)
            soulRatioForOneCredit = 1;
    }

    private void OnShipUnload(int soulsUnloaded)
    {
        AddSouls(soulsUnloaded);
    }

    private void AddSouls(int newSouls)
    {
        _soulsSaved += newSouls;
        _soulBuffer += newSouls;

        AddCredits(_soulBuffer / soulRatioForOneCredit);
        _soulBuffer = _soulBuffer % soulRatioForOneCredit;

        UpdateRessources();
    }

    public void AddBonusCredits(int amount)
    {
        Debug.Log("BONUS CREDITS : " + amount);
        AddCredits(amount);
    }

    public void AddCredits(int amount, bool dontAddToTotal = false)
    {
        _currentCredits += amount;

        if (!dontAddToTotal)
            _totalCreditsEarned += amount;

        UpdateRessources();
    }

    public bool SpendCredits(int amount)
    {
        if (_currentCredits >= amount)
        {
            _currentCredits -= amount;
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
            ressourcesUpdate(_soulsSaved, _soulBuffer, _currentCredits, _totalCreditsEarned);

        if (creditsUpdate != null)
            creditsUpdate(_currentCredits);
    }
}
