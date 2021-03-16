using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Singleton
    public static TimeManager instance;

    [Header("Game time settings")]
    public float startHours;
    public float startMinutes;

    private float _currentTime = 0f;
    private bool _timeStarted;
    private float _timeScaleBeforePause;

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
        GridManager.newGameGrid += StartTime;
    }

    private void OnDisable()
    {
        
    }

    private void StartTime(GridInfo info)
    {
        _currentTime = (startHours * 60f * 60f) + startMinutes * 60f;
        _timeStarted = true;
    }

    private void Update()
    {
        if (_timeStarted)
            _currentTime += (Time.deltaTime * 60f);
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
