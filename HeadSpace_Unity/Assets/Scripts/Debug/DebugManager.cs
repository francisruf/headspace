using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    // Action qui envoie signal quand le tooltip de souris est activé
    public static Action<bool> mouseToolTipActiveState;

    // Singleton
    public static DebugManager instance;

    // Sections de debug
    [Header("Debug references")]
    public GameObject buttonsPanel;
    public GameObject timePanel;
    public TextMeshProUGUI gameTimeText;
    public GameObject objectsPanel;
    public MouseTooltip mouseToolTip;
    public GameObject gridDebug;

    // Command Debug
    [Header("Command Debug references")]
    public GameObject commandDebugWindow;

    // Time stuff
    [Header("Text references")]
    public TextMeshProUGUI pauseButtonText;
    public TextMeshProUGUI timeScaleText;

    // Texte d'instructions
    public TextMeshProUGUI debugText;

    // Prefabs
    [Header("Prefabs")]
    public GameObject genericDocumentPrefab;
    public GameObject[] genericMarkerPrefabs;

    // Time stuff
    private float _timeScaleBeforePause;
    private bool _timePaused;
    private bool _gameTimerStarted;
    private float _gameTimer;

    // State
    private bool _mouseTooltipVisible;

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
    }

    private void OnEnable()
    {
        GridManager.firstAnomalyTile += StartGameTimer;
        GridManager.totalGridAnomaly += StopGameTimer;
    }

    private void OnDisable()
    {
        GridManager.firstAnomalyTile -= StartGameTimer;
        GridManager.totalGridAnomaly -= StopGameTimer;
    }

    private void Start()
    {
        buttonsPanel.SetActive(false);
        timePanel.SetActive(false);
        objectsPanel.SetActive(false);
        debugText.enabled = true;
        gameTimeText.enabled = false;
        commandDebugWindow.SetActive(false);
        UpdateTimeScaleText();
    }

    private void Update()
    {
        if (_gameTimerStarted)
        {
            UpdateGameTimer();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            buttonsPanel.SetActive(!buttonsPanel.activeSelf);
            timePanel.SetActive(!timePanel.activeSelf);
            objectsPanel.SetActive(!objectsPanel.activeSelf);
            debugText.enabled = !buttonsPanel.activeSelf;
            gameTimeText.enabled = !gameTimeText.enabled;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !commandDebugWindow.activeSelf)
        {
            commandDebugWindow.SetActive(true);
        }
    }

    #region Grid management

    public void ToggleGridMode()
    {
        if (GridManager.instance != null)
            GridManager.instance.ToggleGridMode();

        //gridDebug.SetActive(!gridDebug.activeSelf);
    }

    public void ToggleGridObjects()
    {
        if (PlanetManager.instance != null)
            PlanetManager.instance.TogglePlanetDebug();
    }

    public void ToggleMouseCoords()
    {
        _mouseTooltipVisible = !_mouseTooltipVisible;

        if (mouseToolTipActiveState != null)
            mouseToolTipActiveState(_mouseTooltipVisible);
    }

    public void NewGrid()
    {
        if (GridManager.instance != null)
            GridManager.instance.GenerateNewGrid();
    }

    //public void SpawnRandomAnomalyTile()
    //{
    //    if (GridManager.instance != null)
    //        GridManager.instance.SpawnRandomAnomalyTile();
    //}
    #endregion

    #region Time Management

    public void DoubleTimeScale()
    {
        if (_timePaused)
        {
            _timeScaleBeforePause *= 2f;
        }
        else
        {
            Time.timeScale *= 2f;
        }
        UpdateTimeScaleText();
    }

    public void HalveTimeScale()
    {
        if (_timePaused)
        {
            _timeScaleBeforePause /= 2f;
        }
        else
        {
            Time.timeScale /= 2f;
        }
        UpdateTimeScaleText();
    }

    public void ResetTime()
    {
        if (_timePaused)
        {
            _timeScaleBeforePause = 1f;
        }
        else
        {
            Time.timeScale = 1f;
        }
        UpdateTimeScaleText();
    }

    public void PauseTime()
    {
        if (!_timePaused)
        {
            _timePaused = true;

            _timeScaleBeforePause = Time.timeScale;
            pauseButtonText.text = "Resume";
            Time.timeScale = 0f;
        }
        else
        {
            _timePaused = false;

            Time.timeScale = _timeScaleBeforePause;
            pauseButtonText.text = "Pause";
        }
        UpdateTimeScaleText();
    }

    private void UpdateTimeScaleText()
    {
        if (_timePaused)
        {
            string pauseText = "<color=red>(PAUSED) </color>";
            timeScaleText.text = pauseText + "TimeScale : " + _timeScaleBeforePause;
        }
        else
        {
            timeScaleText.text = "TimeScale : " + Time.timeScale;
        }
    }

    private void StartGameTimer()
    {
        ResetGameTimer();
        //Debug.Log("TIMER START");
        _gameTimerStarted = true;
    }

    private void ResetGameTimer()
    {
        _gameTimer = 0f;
    }

    private void UpdateGameTimer()
    {
        _gameTimer += Time.deltaTime;
        gameTimeText.text = "Game Time : " + TimeSpan.FromSeconds(_gameTimer).ToString(@"mm\:ss");
    }

    private void StopGameTimer()
    {
        Debug.Log("TIMER STOP");
        _gameTimerStarted = false;
    }

    #endregion

    #region Object management

    public void SpawnDocument()
    {
        Vector3 spawnPos = Camera.main.transform.position;
        spawnPos.z = 0f;

        GameObject doc = Instantiate(genericDocumentPrefab, spawnPos, Quaternion.identity);
    }

    public void SpawnMarker()
    {
        Vector3 spawnPos = new Vector3(-4.36f, -4.8f, 0f);
        int randomIndex = UnityEngine.Random.Range(0, genericMarkerPrefabs.Length);
        GameObject marker = Instantiate(genericMarkerPrefabs[randomIndex], spawnPos, Quaternion.identity);
    }

    #endregion
}
