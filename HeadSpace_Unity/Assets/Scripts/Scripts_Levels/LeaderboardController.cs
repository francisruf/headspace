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
    public List<Sprite> leaderboardDayPosters;
    public SpriteRenderer posterRenderer;
    public TextMeshProUGUI messageBoard;
    public Sprite winPoster;
    public Sprite losePoster;

    [Header("EmployeeSlots")]
    public Transform[] employeePos;
    public GameObject characterPrefab;

    [Header("LIGHTS")]
    public GameObject dayLights;
    public GameObject nightLights;
    public GameObject debugLights;

    private List<Employee> _employees = new List<Employee>();
    private Employee _player;
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
        AssignCustomInformation();
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
        //Debug.Log("TOTAL CREDITS : " + RessourceManager.instance.TotalCredits + " (+" + GameManager.instance.LastSectorInfo.CreditsGained);
    }

    private void AssignLeaderboard()
    {
        if (EmployeeManager.instance == null)
            return;

        _employees = EmployeeManager.instance.SortedEmployees;
        _player = EmployeeManager.instance.player;
        int count = _employees.Count;

        for (int i = 0; i < 4 && i < count; i++)
        {
            EmployeeSlot slot = Instantiate(characterPrefab, employeePos[i]).GetComponent<EmployeeSlot>();
            slot.transform.localPosition = Vector2.zero;
            slot.InitializeSlot(GameManager.instance.CurrentDayInfo.time, _employees[i]);
        }

        if (leaderboardLoaded != null)
            leaderboardLoaded();
    }

    private void AssignCustomInformation()
    {
        int day = _currentInfo.day;
        if (day < leaderboardDayPosters.Count)
        {
            posterRenderer.sprite = leaderboardDayPosters[day];
        }

        if (day == 1 && _currentInfo.time == LevelTime.NightEnd)
        {
            if (_employees[_employees.Count - 1] == _player)
            {
                posterRenderer.sprite = losePoster;
            }
            else if (_employees[0] == _player)
            {
                posterRenderer.sprite = winPoster;
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