using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static Action baseLoadingDone;
    public static Action loadingDone;
    
    // Singleton
    public static LevelManager instance;

    // Scenes
    public string[] baseScenes;
    public string[] mainMenuScenes;
    public string[] essentialLevelScenes;
    public string[] environmentScenes;

    // Run-time scene tracking
    private string[] _currentMenuScenes;
    private string _currentLevelScene;

    // Sector info
    private SectorInfo _previousSectorInfo;
    public SectorInfo PreviousSectorInfo { get { return _previousSectorInfo; } }

    private IEnumerator _currentLoadingRoutine;

    private void Awake()
    {
        // Assigner le singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.gameEnded += OnLevelTimerEnded;
        GameManager.gameOver += OnGameOver;
        MainMenuController.playButtonPressed += OnPlayButtonPressed;
        MainMenuController.quitButtonPressed += OnQuitButtonPressed;
        EndMenuController.playAgainButtonPressed += OnPlayAgainButtonPressed;
        EndMenuController.quitButtonPressed += OnQuitButtonPressed;
        SectorManager.sectorInfoUpdate += OnSectorInfoUpdate;
    }

    private void OnDisable()
    {
        GameManager.gameEnded -= OnLevelTimerEnded;
        GameManager.gameOver -= OnGameOver;
        MainMenuController.playButtonPressed -= OnPlayButtonPressed;
        MainMenuController.quitButtonPressed -= OnQuitButtonPressed;
        EndMenuController.playAgainButtonPressed -= OnPlayAgainButtonPressed;
        EndMenuController.quitButtonPressed -= OnQuitButtonPressed;
        SectorManager.sectorInfoUpdate -= OnSectorInfoUpdate;
    }

    private void Start()
    {
        if (GameManager.instance == null)
        {
            StartCoroutine(LoadStartScenes());
        }
    }

    private IEnumerator LoadStartScenes()
    {
        _currentMenuScenes = mainMenuScenes;

        yield return LoadScenes(baseScenes);
        yield return LoadScenes(_currentMenuScenes);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScenes[0]));

        if (baseLoadingDone != null)
            baseLoadingDone();
    }

    private void OnPlayButtonPressed()
    {
        StartCoroutine(LoadLevelScenes("00_Gym"));
    }

    private void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    private void OnPlayAgainButtonPressed()
    {
        StartCoroutine(LoadLevelScenes("00_Gym"));
    }


    private void OnLevelTimerEnded()
    {
        StartCoroutine(LoadMenuSceneFromGame("EndScreen"));
    }

    private void OnGameOver()
    {
        StartCoroutine(LoadMenuSceneFromGame("GameOverScreen"));
    }

    private IEnumerator LoadLevelScenes(string targetLevelName)
    {
        if (_currentMenuScenes != null)
        {
            yield return UnloadScenes(_currentMenuScenes);
            _currentMenuScenes = null;
        }

        _currentLevelScene = targetLevelName;

        yield return LoadScenes(essentialLevelScenes);
        yield return LoadScenes(environmentScenes);
        yield return LoadScenes(_currentLevelScene);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentLevelScene));

        if (loadingDone != null)
            loadingDone();
    }

    private IEnumerator LoadMenuSceneFromGame(string targetScreenName)
    {
        _currentMenuScenes = new string[] { targetScreenName };

        yield return UnloadScenes(essentialLevelScenes);
        yield return UnloadScenes(environmentScenes);
        yield return UnloadScenes(_currentLevelScene);
        yield return LoadScenes(_currentMenuScenes);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetScreenName));

        if (loadingDone != null)
            loadingDone();
    }

    private IEnumerator UnloadScenes(params string[] scenes)
    {
        foreach (var scene in scenes)
        {
            SceneManager.UnloadSceneAsync(scene);
        }
        yield return new WaitForSeconds(0f);
    }

    private IEnumerator LoadScenes(params string[] scenes)
    {
        //SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Additive);

        //// Doit attendre un peu a cause d'un qwerk stupide de unity avec allowSceneActivation
        //// allowSceneActivation va etre ignoré si se passe au même frame qu'une scene TERMINE de se loader
        yield return new WaitForSeconds(0.1f);

        List<AsyncOperation> ops = new List<AsyncOperation>();

        foreach (var scene in scenes)
        {
            // LoadScene retourne une AsyncOperation
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            Debug.Log("Loading : " + scene);

            // N'active pas la scene avant qu'elle load
            op.allowSceneActivation = false;
            ops.Add(op);

            // Activation de la scene se fait dans le dernier 0.1
            while (op.progress < 0.9f)
            {
                yield return new WaitForSeconds(0);
            }
        }

        // Attente input pour loader

        foreach (AsyncOperation op in ops)
        {
            op.allowSceneActivation = true;
        }
        yield return new WaitForSeconds(0);

        _currentLoadingRoutine = null;
        //SceneManager.UnloadSceneAsync("LoadingScreen");
    }

    private void OnSectorInfoUpdate(SectorInfo sectorInfo)
    {
        _previousSectorInfo = sectorInfo;
    }

}

