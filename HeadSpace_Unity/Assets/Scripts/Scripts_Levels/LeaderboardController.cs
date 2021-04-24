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
    public GameObject debugLights;
    private DayInfo _currentInfo = default;

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
        
        if (GameManager.instance != null)
            _currentInfo = GameManager.instance.CurrentDayInfo;

        switch (_currentInfo.time)
        {
            case LevelTime.DayStart:
                AssignDayInfo();
                break;
            case LevelTime.NightEnd:
                AssignNightInfo();
                break;
        }
    }

    private void AssignDayInfo()
    {
        debugLights.SetActive(false);
        nightLights.SetActive(false);
        dayLights.SetActive(true);
    }

    private void AssignNightInfo()
    {
        debugLights.SetActive(false);
        dayLights.SetActive(false);
        nightLights.SetActive(true);
        Debug.Log("TOTAL CREDITS : " + RessourceManager.instance.TotalCredits + " (+" + GameManager.instance.LastSectorInfo.CreditsGained);
    }

    private void OnCardProcessed()
    {
        StartCoroutine(EndOfScene());
    }

    private IEnumerator EndOfScene()
    {
        yield return new WaitForSeconds(0.5f);

        switch (_currentInfo.time)
        {
            case LevelTime.DayStart:
                if (dayStart != null)
                    dayStart();
                break;

            case LevelTime.NightEnd:
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