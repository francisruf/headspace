using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    public static Action leaderboardLoaded;
    public static Action dayStart;
    public static Action dayFinish;

    [Header("Dynamic Info")]
    public List<LeaderboardDayInfo> customDayInformation;
    public SpriteRenderer posterRenderer;
    public TextMeshProUGUI messageBoard;

    [Header("EmployeeSlots")]
    public Transform[] employeePos;
    public GameObject characterPrefab;

    [Header("LIGHTS")]
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

        AssignLeaderboard();
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

    private void AssignLeaderboard()
    {
        if (EmployeeManager.instance == null)
            return;

        List<Employee> employees = EmployeeManager.instance.SortedEmployees;
        int count = employees.Count;

        for (int i = 0; i < 4 && i < count; i++)
        {
            EmployeeSlot slot = Instantiate(characterPrefab, employeePos[i]).GetComponent<EmployeeSlot>();
            slot.transform.localPosition = Vector2.zero;
            slot.InitializeSlot(GameManager.instance.CurrentDayInfo.time, employees[i]);
        }

        if (leaderboardLoaded != null)
            leaderboardLoaded();
    }

    private void AssignCustomInformation()
    {
        int day = _currentInfo.day;
        if (day < customDayInformation.Count)
        {
            if (_currentInfo.time == LevelTime.DayStart)
            {
                posterRenderer.sprite = customDayInformation[day].dayPoster;
                messageBoard.text = customDayInformation[day].dayMessage;
            }
            else
            {
                posterRenderer.sprite = customDayInformation[day].nightPoster;
                messageBoard.text = customDayInformation[day].nightMessage;
            }
        }
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

[System.Serializable]
public struct LeaderboardDayInfo
{
    public string dayMessage;
    public string nightMessage;

    public Sprite dayPoster;
    public Sprite nightPoster;
}