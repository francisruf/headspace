using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static Action loadingDone;
    
    // Singleton
    public static LevelManager instance;

    public List<Scene> scenesToLoad;

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
        }
    }

    private void Start()
    {
        string[] sceneNames = new string[scenesToLoad.Count];

        for (int i = 0; i < sceneNames.Length; i++)
        {
            sceneNames[i] = scenesToLoad[i].name;
        }
        LoadScenes(sceneNames);
    }

    private IEnumerator LoadScenes(params string[] scenes)
    {
        //SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Additive);

        //// Doit attendre un peu a cause d'un qwerk stupide de unity avec allowSceneActivation
        //// allowSceneActivation va etre ignoré si se passe au même frame qu'une scene TERMINE de se loader
        //yield return new WaitForSeconds(0.1f);

        List<AsyncOperation> ops = new List<AsyncOperation>();

        foreach (var scene in scenes)
        {
            // LoadScene retourne une AsyncOperation
            AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

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
        bool input = false;
        while (!input)
        {
            if (Input.anyKeyDown)
            {
                input = true;
                foreach (AsyncOperation op in ops)
                {
                    op.allowSceneActivation = true;
                }
            }
            yield return new WaitForSeconds(0);
        }

        //SceneManager.UnloadSceneAsync("LoadingScreen");
    }
}

