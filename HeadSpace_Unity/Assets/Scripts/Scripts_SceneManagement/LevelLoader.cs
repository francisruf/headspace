using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static Action<int> levelLoaded;

    private PlayerInventory _inventory;
    private LevelSettings _levelSettings;

    public Canvas startGameDebug;
    public int levelID;

    private void Awake()
    {
        _levelSettings = GetComponentInChildren<LevelSettings>();
        
        if (LevelManager.instance != null)
            startGameDebug.enabled = false;
        else
            startGameDebug.enabled = true;
    }

    private void OnEnable()
    {
        LevelManager.preLoadDone += OnLoadingComplete;
    }

    private void OnDisable()
    {
        LevelManager.preLoadDone -= OnLoadingComplete;
    }

    private void OnLoadingComplete()
    {
        LevelEndCondition condition = LevelEndCondition.Time;
        if (_levelSettings != null)
            condition = _levelSettings.levelEndCondition;

        if (GameManager.instance != null)
            GameManager.instance.PrepareLevel(condition);

        if (startGameDebug != null)
            startGameDebug.enabled = false;

        if (_levelSettings != null)
            AssignLevelSettings();

        if (GridManager.instance != null)
            GridManager.instance.GenerateNewGrid();

        //if (ShipManager.instance != null)
        //    ShipManager.instance.PlaceAllShipsOnMap();

        if (_levelSettings != null)
            if (_levelSettings.inventorySettings != null)
                _levelSettings.inventorySettings.InitializeInventory();

        if (levelLoaded != null)
            levelLoaded(levelID);
    }

    private void AssignLevelSettings()
    {
        if (_levelSettings.gridSettings != null)
            if (GridManager.instance != null)
                GridManager.instance.AssignSettings(_levelSettings.gridSettings);

        if (_levelSettings.planetLevelSettings != null)
        {
            if (PlanetManager.instance != null)
                PlanetManager.instance.AssignLevelSettings(_levelSettings.planetLevelSettings);

            if (ShipManager.instance != null)
                ShipManager.instance.shipHeatDistance = _levelSettings.planetLevelSettings.planetHeatDistance;
        }
            
        if (ContractManager.instance != null)
            ContractManager.instance.AssignLevelSettings(_levelSettings.contractSettings);
    }

    public void StartGameDebug()
    {
        OnLoadingComplete();
    }
}
