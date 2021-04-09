using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private PlayerInventory _inventory;

    public Canvas startGameDebug;

    private void Awake()
    {
        _inventory = FindObjectOfType<PlayerInventory>();
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
    }

    public void StartGameDebug()
    {
        OnLoadingComplete();
    }
}
