﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static Action levelTimerEnded;
    public static Action levelTimerEndPreTrigger;

    // Singleton
    public static TimeManager instance;

    [Header("Game time settings")]
    public float startHours;
    public float startMinutes;
    public float timeMultiplier = 1.25f;

    [Header("Timer settings")]
    public float hurryUpHours;

    private float _currentTime = 0f;
    private bool _timeStarted;
    private bool _levelEnded;
    private bool _triggerFired;
    private float _timeScaleBeforePause;

    private float _levelEndTimer;
    private float _endPreTrigger;

    public float GetCurrentTime
    {
        get
        {
            if (!GameManager.GameStarted)
            {
                return (startHours * 60f * 60f) + startMinutes * 60f;
            }
            else
            {
                return _currentTime;
            }
        }
    }

    public float GameTime { get { return _currentTime; } }

    private void Awake()
    {
        // Déclaration du singleton
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
        _currentTime = (startHours * 60f * 60f) + startMinutes * 60f;
        GameManager.levelStarted += StartTime;
    }

    private void OnDisable()
    {
        GameManager.levelStarted -= StartTime;
    }

    private void StartTime()
    {
        if (GameManager.instance != null)
        {
            _levelEndTimer = _currentTime + (GameManager.instance.LevelDurationInMinutes * 60f * 60f * timeMultiplier);
            _endPreTrigger = _levelEndTimer - 2.15f * 60f * timeMultiplier;
        }
        _timeStarted = true;
    }

    private void Update()
    {
        if (_timeStarted)
        {
            _currentTime += (Time.deltaTime * 60f * timeMultiplier);

            if (!_triggerFired && _currentTime >= _endPreTrigger)
            {
                _triggerFired = true;
                if (levelTimerEndPreTrigger != null)
                    levelTimerEndPreTrigger();
            }

            if (_currentTime >= _levelEndTimer)
            {
                if (!_levelEnded)
                {
                    _timeStarted = false;
                    _currentTime = _levelEndTimer;
                    _levelEnded = true;

                    if (levelTimerEnded != null)
                        levelTimerEnded();
                }

            }
        }
    }

    //public void DoubleTimeScale()
    //{
    //    Time.timeScale *= 2f;
    //}

    //public void HalveTimeScale()
    //{
    //    Time.timeScale /= 2f;
    //}
    
    //public void ResetTime()
    //{
    //    Time.timeScale = 1f;
    //}

    //public void PauseTime()
    //{
    //    _timeScaleBeforePause = Time.timeScale;
    //    Time.timeScale = 0f;
    //}

    //public void ResumeTime()
    //{
    //    Time.timeScale = _timeScaleBeforePause;
    //}
}
