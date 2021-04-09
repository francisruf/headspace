using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Action gameStarted;
    public static Action gameEnded;
    public static Action gameOver;
    public static bool GameStarted { get; private set; }

    public float LevelDurationInMinutes { get { return levelDurationInMinutes; } }

    [Header("Level duration")]
    public float levelDurationInMinutes;

    private int _minimumCreditsNeeded;

    private void Awake()
    {
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
        GameStarted = false;

        Command_Send.playerFirstDeploy += OnFirstDeploy;
        TimeManager.levelTimerEnded += OnLevelTimerEnded;
        PlanetManager.noMoreSoulsInSector += OnNoMoreSoulsInSector;
    }

    private void OnDisable()
    {
        Command_Send.playerFirstDeploy -= OnFirstDeploy;
        TimeManager.levelTimerEnded -= OnLevelTimerEnded;
        PlanetManager.noMoreSoulsInSector -= OnNoMoreSoulsInSector;
    }

    private void OnFirstDeploy()
    {
        if (BuyablesDatabase.instance != null)
            _minimumCreditsNeeded = BuyablesDatabase.instance.GetMinimumShipPrice();
        else
            _minimumCreditsNeeded = 0;

        GameStarted = true;
        
        StartCoroutine(CheckPlayerRessources());

        if (gameStarted != null)
            gameStarted();
    }

    public void ForceStartGame()
    {
        OnFirstDeploy();
    }

    private void OnLevelTimerEnded()
    {
        EndSector();
    }

    private void OnNoMoreSoulsInSector()
    {
        EndSector();
    }

    private void EndSector()
    {
        GameStarted = false;

        if (gameEnded != null)
            gameEnded();
    }

    private void GameOver()
    {
        GameStarted = false;

        if (gameOver != null)
            gameOver();
    }

    private IEnumerator CheckPlayerRessources()
    {
        bool hasShips = false;
        bool hasEnoughCredits = false;

        while (GameStarted)
        {    
            yield return new WaitForSeconds(1f);

            hasShips = true;
            hasEnoughCredits = true;

            if (RessourceManager.instance != null)
                if (RessourceManager.instance.CurrentCredits < _minimumCreditsNeeded)
                    hasEnoughCredits = false;

            if (ShipManager.instance != null)
                if (ShipManager.instance.ActiveShipsCount <= 0)
                    hasShips = false;

            if (ShopManager.instance != null)
                if (!ShopManager.instance.TransactionInProgress)
                {
                    if (hasShips == false && hasEnoughCredits == false)
                    {
                        GameOver();
                        break;
                    }
                }
        }
    }
}
