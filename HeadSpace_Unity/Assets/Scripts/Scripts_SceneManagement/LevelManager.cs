using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static Action baseLoadingDone;
    public static Action loadingDone;
    public static Action unloadingDone;

    // Singleton
    public static LevelManager instance;

    // Scenes
    [Header("SCENES")]
    public string[] baseScenes;
    public string[] mainMenuScenes;
    public string[] essentialLevelScenes;
    public string[] environmentScenes;

    // Run-time scene tracking
    private string[] _currentMenuScenes;
    private string _currentLevelScene;
    private string _currentSingleScene = "";

    // Sector info
    private SectorInfo _previousSectorInfo;
    public SectorInfo PreviousSectorInfo { get { return _previousSectorInfo; } }

    private IEnumerator _currentLoadingRoutine;

    // Scene transitions
    [Header("TRANSITIONS")]
    public Image blackSolid;
    public Canvas transitionCanvas;
    public float fadeSpeed;

    private int _currentDay;

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
        GameManager.levelEnded += OnLevelTimerEnded;
        GameManager.gameOver += OnGameOver;
        MainMenuController.playButtonPressed += OnPlayButtonPressed;
        MainMenuController.quitButtonPressed += OnQuitButtonPressed;
        EndMenuController.playAgainButtonPressed += OnPlayAgainButtonPressed;
        EndMenuController.quitButtonPressed += OnQuitButtonPressed;
        SectorManager.sectorInfoUpdate += OnSectorInfoUpdate;
        CutsceneController.cutsceneOver += OnCutsceneOver;
        DaySceneController.daySceneOver += OnDaySceneOver;
        LeaderboardController.dayStart += OnDayStart;
        TutorialPromptController.tutorialPrompt += OnTutorialPrompt;
    }

    private void OnDisable()
    {
        GameManager.levelEnded -= OnLevelTimerEnded;
        GameManager.gameOver -= OnGameOver;
        MainMenuController.playButtonPressed -= OnPlayButtonPressed;
        MainMenuController.quitButtonPressed -= OnQuitButtonPressed;
        EndMenuController.playAgainButtonPressed -= OnPlayAgainButtonPressed;
        EndMenuController.quitButtonPressed -= OnQuitButtonPressed;
        SectorManager.sectorInfoUpdate -= OnSectorInfoUpdate;
        CutsceneController.cutsceneOver -= OnCutsceneOver;
        DaySceneController.daySceneOver -= OnDaySceneOver;
        LeaderboardController.dayStart -= OnDayStart;
        TutorialPromptController.tutorialPrompt -= OnTutorialPrompt;
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
        blackSolid.enabled = true;
        transitionCanvas.enabled = true;

        _currentMenuScenes = mainMenuScenes;

        yield return LoadScenes(baseScenes);
        yield return LoadScenes(_currentMenuScenes);

        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainMenuScenes[0]));

        yield return FadeIn();

        if (baseLoadingDone != null)
            baseLoadingDone();
    }

    private void OnPlayButtonPressed()
    {
        StartCoroutine(LoadSingleScene("Cutscene_Intro", 0.5f));
        //StartCoroutine(LoadLevelScenes("00_Gym"));
    }

    private void OnQuitButtonPressed()
    {
        Application.Quit();
    }


    private void OnPlayAgainButtonPressed()
    {
        StartCoroutine(LoadLevelScenes("00_Gym", 0.5f));
    }

    private void OnLevelTimerEnded()
    {
        StartCoroutine(LoadSingleSceneFromGame("Leaderboard", 0.5f));
    }

    private void OnGameOver()
    {
        StartCoroutine(LoadMenuSceneFromGame("GameOverScreen", 0f));
    }

    private void OnTutorialPrompt(bool tutorial)
    {
        int day = tutorial == true ? 0 : 1;
        GameManager.instance.SetDay(day);
        StartCoroutine(LoadSingleScene("Leaderboard", 1f));
    }

    private void OnDaySceneOver()
    {
        StartCoroutine(LoadSingleScene("Leaderboard", 1f));
    }

    private void OnDayStart()
    {
        Debug.Log("On day start!");
        StartCoroutine(LoadLevelScenes("00_Gym", 1f));
    }

    private void OnDayFinish()
    {
        Debug.Log("On day finish!");
        StartCoroutine(LoadSingleScene("DayMenu", 1f));
    }

    private IEnumerator LoadLevelScenes(string targetLevelName, float timeBeforeFade)
    {
        yield return FadeOut();

        if (_currentMenuScenes != null)
        {
            yield return UnloadScenes(_currentMenuScenes);
            _currentMenuScenes = null;
        }

        if (_currentSingleScene != "")
        {
            yield return UnloadScenes(_currentSingleScene);
            _currentSingleScene = "";
        }

        if (unloadingDone != null)
            unloadingDone();

        _currentLevelScene = targetLevelName;
        yield return LoadScenes(essentialLevelScenes);
        yield return LoadScenes(environmentScenes);
        yield return LoadScenes(_currentLevelScene);
        yield return FadeIn(timeBeforeFade);

        if (loadingDone != null)
            loadingDone();
    }

    private IEnumerator LoadSingleScene(string targetScene, float timeBeforeFade)
    {
        yield return FadeOut();

        if (_currentMenuScenes != null)
        {
            yield return UnloadScenes(_currentMenuScenes);
            _currentMenuScenes = null;
        }

        if (_currentSingleScene != "")
        {
            yield return UnloadScenes(_currentSingleScene);
            _currentSingleScene = "";
        }

        if (unloadingDone != null)
            unloadingDone();

        _currentSingleScene = targetScene;
        yield return LoadScenes(_currentSingleScene);
        yield return FadeIn(timeBeforeFade);

        if (loadingDone != null)
            loadingDone();
    }


    private IEnumerator LoadMenuSceneFromGame(string targetScreenName, float timeBeforeFade)
    {
        _currentMenuScenes = new string[] { targetScreenName };

        yield return FadeOut();
        yield return UnloadScenes(essentialLevelScenes);
        yield return UnloadScenes(environmentScenes);
        yield return UnloadScenes(_currentLevelScene);

        if (unloadingDone != null)
            unloadingDone();

        yield return LoadScenes(_currentMenuScenes);
        yield return FadeIn(timeBeforeFade);

        if (loadingDone != null)
            loadingDone();
    }

    private IEnumerator LoadSingleSceneFromGame(string targetScreenName, float timeBeforeFade)
    {
        _currentSingleScene = targetScreenName;

        yield return FadeOut();
        yield return UnloadScenes(essentialLevelScenes);
        yield return UnloadScenes(environmentScenes);
        yield return UnloadScenes(_currentLevelScene);

        if (unloadingDone != null)
            unloadingDone();

        yield return LoadScenes(targetScreenName);
        yield return FadeIn(timeBeforeFade);

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

    private IEnumerator FadeOut()
    {
        Color newColor = blackSolid.color;
        newColor.a = 0f;
        blackSolid.color = newColor;
        blackSolid.enabled = true;
        transitionCanvas.enabled = true;

        while (blackSolid.color.a < 0.99f)
        {
            newColor.a += Time.deltaTime * fadeSpeed;
            blackSolid.color = newColor;
            yield return new WaitForEndOfFrame();
        }

        newColor.a = 1f;
        blackSolid.color = newColor;
    }

    private IEnumerator FadeIn(float timeBeforeFade = 0f)
    {
        yield return new WaitForSeconds(timeBeforeFade);

        Color newColor = blackSolid.color;
        newColor.a = 1f;
        blackSolid.color = newColor;
        blackSolid.enabled = true;
        transitionCanvas.enabled = true;

        while (blackSolid.color.a > 0.01f)
        {
            newColor.a -= Time.deltaTime * fadeSpeed;
            blackSolid.color = newColor;
            yield return new WaitForEndOfFrame();
        }

        newColor.a = 0f;
        blackSolid.color = newColor;

        blackSolid.enabled = false;
        transitionCanvas.enabled = false;
    }

    private void OnCutsceneOver(string nextSceneName, SceneLoadType sceneLoadType)
    {
        switch (sceneLoadType)
        {
            case SceneLoadType.SingleScene:
                StartCoroutine(LoadSingleScene(nextSceneName, 1f));
                break;
            case SceneLoadType.MenuFromGame:
                StartCoroutine(LoadMenuSceneFromGame(nextSceneName, 1f));
                break;
            case SceneLoadType.Level:
                StartCoroutine(LoadLevelScenes(nextSceneName, 1f));
                break;
            default:
                break;
        }
    }

}

public enum SceneLoadType
{
    SingleScene,
    MenuFromGame,
    Level
}

