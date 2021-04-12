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
        startGameDebug.enabled = true;
    }

    private void OnEnable()
    {
        LevelManager.loadingDone += OnLoadingComplete;
    }

    private void OnDisable()
    {
        LevelManager.loadingDone -= OnLoadingComplete;
    }

    private void OnLoadingComplete()
    {
        if (startGameDebug != null)
            startGameDebug.enabled = false;

        if (_inventory != null)
            _inventory.InitializeInventory();

        if (GridManager.instance != null)
            GridManager.instance.GenerateNewGrid();

        if (GameManager.instance != null)
            GameManager.instance.ForceStartGame();

        if (_levelSettings != null)
            AssignLevelSettings();
    }

    private void AssignLevelSettings()
    {
        if (_levelSettings.planetLevelSettings != null)
            if (PlanetManager.instance != null)
                PlanetManager.instance.RevealStartingPlanets(_levelSettings.planetLevelSettings);
    }

    public void StartGameDebug()
    {
        OnLoadingComplete();
    }
}
