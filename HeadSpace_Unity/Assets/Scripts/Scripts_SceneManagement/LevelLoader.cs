using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private PlayerInventory _inventory;
    private LevelSettings _levelSettings;

    public Canvas startGameDebug;

    private void Awake()
    {
        _inventory = FindObjectOfType<PlayerInventory>();
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
        if (GameManager.instance != null)
            GameManager.instance.PrepareLevel();

        if (startGameDebug != null)
            startGameDebug.enabled = false;

        if (_inventory != null)
            _inventory.InitializeInventory();

        if (GridManager.instance != null)
            GridManager.instance.GenerateNewGrid();

        if (_levelSettings != null)
            AssignLevelSettings();
    }

    private void AssignLevelSettings()
    {
        if (_levelSettings.planetLevelSettings != null)
            if (PlanetManager.instance != null)
                PlanetManager.instance.RevealStartingPlanets(_levelSettings.planetLevelSettings);

        if (ContractManager.instance != null)
            ContractManager.instance.AssignLevelSettings(_levelSettings.allClientRules);
    }

    public void StartGameDebug()
    {
        OnLoadingComplete();
    }
}
