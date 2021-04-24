using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Action levelStarted;
    public static Action levelEnded;
    public static Action gameOver;
    public static bool GameStarted { get; private set; }

    public float LevelDurationInMinutes { get { return levelDurationInMinutes; } }

    [Header("Level duration")]
    public float levelDurationInMinutes;

    [Header("Cursor settings")]
    public Sprite baseCursor;
    public Sprite objectCursor;
    public Sprite interactCursor;
    public GameObject cursorObj;
    public LayerMask objectLayers;
    private Image _cursorRenderer;
    public DayInfo CurrentDayInfo;

    private SectorInfo _lastSectorInfo;
    public SectorInfo LastSectorInfo { get { return _lastSectorInfo; } }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        Application.targetFrameRate = 60;
        _cursorRenderer = cursorObj.GetComponent<Image>();
        Cursor.visible = false;

        CurrentDayInfo = new DayInfo();
        CurrentDayInfo.day = 0;
        CurrentDayInfo.time = LevelTime.DayStart;
    }

    private void Update()
    {
        MoveCursor();
        SetCursor();
    }

    private void OnEnable()
    {
        GameStarted = false;
        TimeManager.levelTimerEnded += OnLevelTimerEnded;
        LeaderboardController.dayStart += OnDayStart;
        LeaderboardController.dayFinish += OnDayFinish;
    }

    private void OnDisable()
    {
        TimeManager.levelTimerEnded -= OnLevelTimerEnded;
        LeaderboardController.dayStart -= OnDayStart;
        LeaderboardController.dayFinish -= OnDayFinish;
    }

    public void StartLevel()
    {
        GameStarted = true;

        if (levelStarted != null)
            levelStarted();

        Debug.Log("GAME STARTED");
    }

    private void OnLevelTimerEnded()
    {
        if (SectorManager.instance != null)
            _lastSectorInfo = SectorManager.instance.CurrentSectorInfo;

        if (levelEnded != null)
            levelEnded();
    }

    private void GameOver()
    {
        GameStarted = false;

        if (gameOver != null)
            gameOver();
    }

    private void MoveCursor()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorObj.transform.position = Input.mousePosition;
    }

    private void SetCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, objectLayers);

        bool overObject = false;
        bool interactionZone = false;
        int length = hitsInfo.Length;

        if (length > 0)
            overObject = true;

        for (int i = 0; i < length; i++)
        {
            if (hitsInfo[i].collider.GetComponent<ObjectInteractionZone>() != null)
            {
                interactionZone = true;
                break;
            }
        }

        if (_cursorRenderer != null)
        {
            if (interactionZone)
                _cursorRenderer.sprite = interactCursor;
            else if (overObject)
                _cursorRenderer.sprite = objectCursor;
            else
                _cursorRenderer.sprite = baseCursor;
        }
    }

    public void SetDay(int newDay)
    {
        CurrentDayInfo.day = newDay;
    }

    private void OnDayStart()
    {
        CurrentDayInfo.time = LevelTime.NightEnd;
    }

    private void OnDayFinish()
    {
        CurrentDayInfo.day++;
        CurrentDayInfo.time = LevelTime.DayStart;
    }
}

public enum LevelTime
{
    DayStart,
    NightEnd
}

public struct DayInfo
{
    public int day;
    public LevelTime time;
}
