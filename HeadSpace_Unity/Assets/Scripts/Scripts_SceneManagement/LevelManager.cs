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
    public string[] baseScenes;
    public string[] mainMenuScenes;
    public string[] gymScenes;

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
        MainMenuController.playButtonPressed += OnPlayButtonPressed;
        MainMenuController.quitButtonPressed += OnQuitButtonPressed;
    }

    private void OnDisable()
    {
        MainMenuController.playButtonPressed -= OnPlayButtonPressed;
        MainMenuController.quitButtonPressed -= OnQuitButtonPressed;
    }

    private void Start()
    {
        StartCoroutine(LoadStartScenes());
    }

    #region Application start

    private IEnumerator LoadStartScenes()
    {
        yield return LoadScenes(baseScenes);
        yield return LoadScenes(mainMenuScenes);
        
        if (baseLoadingDone != null)
            baseLoadingDone();
    }

    #endregion
    #region Main menu

    private void OnPlayButtonPressed()
    {
        StartCoroutine(TryLoadScenes(gymScenes, mainMenuScenes));
    }

    private void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    #endregion
    #region Scene loading

    private IEnumerator TryLoadScenes(string[] scenesToLoad, string[] scenesToUnoad)
    {
        yield return UnloadScenes(scenesToUnoad);
        yield return LoadScenes(scenesToLoad);
    }

    private IEnumerator TryLoadScenes(string[] scenesToLoad)
    {
        yield return LoadScenes(scenesToLoad);
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

        if (loadingDone != null)
            loadingDone();

        // Attente input pour loader

        foreach (AsyncOperation op in ops)
        {
            op.allowSceneActivation = true;
        }
        yield return new WaitForSeconds(0);

        _currentLoadingRoutine = null;
        //SceneManager.UnloadSceneAsync("LoadingScreen");
    }
    
    #endregion
}

