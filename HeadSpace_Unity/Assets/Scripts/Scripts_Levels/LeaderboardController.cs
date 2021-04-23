using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    public static Action dayStart;
    public static Action dayFinish;
    public leaderboardSceneType sceneType;

    public GameObject dayLights;
    public GameObject nightLights;

    private void OnEnable()
    {
        DropZone_CardReader.cardProcessed += OnCardProcessed;
    }

    private void OnDisable()
    {
        DropZone_CardReader.cardProcessed -= OnCardProcessed;
    }

    private void Awake()
    {
        switch (sceneType)
        {
            case leaderboardSceneType.Day:
                AssignDayInfo();
                break;
            case leaderboardSceneType.Night:
                AssignNightInfo();
                break;
        }
    }

    private void AssignDayInfo()
    {
        nightLights.SetActive(false);
        dayLights.SetActive(true);
    }

    private void AssignNightInfo()
    {
        dayLights.SetActive(false);
        nightLights.SetActive(true);
    }

    private void OnCardProcessed()
    {
        StartCoroutine(EndOfScene());
    }

    private IEnumerator EndOfScene()
    {
        yield return new WaitForSeconds(0.5f);

        switch (sceneType)
        {
            case leaderboardSceneType.Day:
                Debug.Log("DAY TYPE");
                if (dayStart != null)
                    dayStart();
                break;

            case leaderboardSceneType.Night:
                if (dayFinish != null)
                    dayFinish();
                break;
        }
    }
}
public enum leaderboardSceneType
{
    Day,
    Night
}