using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DaySceneController : MonoBehaviour
{
    public static Action daySceneOver;

    [Header("Scene duration without transitions")]
    public float sceneTimer;

    private TextMeshProUGUI _dayText;

    private void OnEnable()
    {
        LevelManager.loadingDone += StartScene;
    }

    private void OnDisable()
    {
        LevelManager.loadingDone -= StartScene;
    }

    private void Awake()
    {
        _dayText = GetComponentInChildren<TextMeshProUGUI>();

        int currentDay = 0;
        if (GameManager.instance != null)
            currentDay = GameManager.instance.CurrentDayInfo.day;

        if (currentDay == 0)
        {
            _dayText.text = "DAY 0: TRAINING";
        }
        else
        {
            _dayText.text = "DAY " + currentDay;
        }
    }

    private void StartScene()
    {
        StartCoroutine(SceneTimer());
    }

    private IEnumerator SceneTimer()
    {
        Debug.Log("Scene start");
        yield return new WaitForSeconds(sceneTimer);

        Debug.Log("Scene end");
        if (daySceneOver != null)
            daySceneOver();
    }
}
