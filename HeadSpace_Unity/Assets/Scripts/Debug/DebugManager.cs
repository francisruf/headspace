﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    // Singleton
    public static DebugManager instance;

    // Sections de debug
    [Header("Debug references")]
    public GameObject buttonsPanel;
    public GameObject timePanel;
    public GameObject objectsPanel;
    public GameObject mouseToolTip;
    public GameObject gridDebug;

    // Time stuff
    [Header("Text references")]
    public TextMeshProUGUI pauseButtonText;
    public TextMeshProUGUI timeScaleText;

    // Texte d'instructions
    public TextMeshProUGUI debugText;

    // Prefabs
    [Header("Prefabs")]
    public GameObject genericDocumentPrefab;

    private float _timeScaleBeforePause;
    private bool _timePaused;


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

    private void Start()
    {
        buttonsPanel.SetActive(false);
        timePanel.SetActive(false);
        objectsPanel.SetActive(false);
        debugText.enabled = true;
        UpdateTimeScaleText();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            buttonsPanel.SetActive(!buttonsPanel.activeSelf);
            timePanel.SetActive(!timePanel.activeSelf);
            objectsPanel.SetActive(!objectsPanel.activeSelf);
            debugText.enabled = !buttonsPanel.activeSelf;
        }
    }

    #region Grid management

    public void ToggleGrid()
    {
        gridDebug.SetActive(!gridDebug.activeSelf);
    }

    public void ToggleMouseCoords()
    {
        mouseToolTip.SetActive(!mouseToolTip.activeSelf);
    }

    public void SpawnRandomAnomalyTile()
    {
        if (GridManager.instance != null)
            GridManager.instance.SpawnRandomAnomalyTile();
    }
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
    #endregion

    #region Object management

    public void SpawnDocument()
    {
        Vector3 spawnPos = Camera.main.transform.position;
        spawnPos.z = 0f;

        GameObject doc = Instantiate(genericDocumentPrefab, spawnPos, Quaternion.identity);
    }

    #endregion
}
