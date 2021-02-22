using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    private float _timeScaleBeforePause;


    public void DoubleTimeScale()
    {
        Time.timeScale *= 2f;
    }

    public void HalveTimeScale()
    {
        Time.timeScale /= 2f;
    }
    
    public void ResetTime()
    {
        Time.timeScale = 1f;
    }

    public void PauseTime()
    {
        _timeScaleBeforePause = Time.timeScale;
        Time.timeScale = 0f;
    }

    public void ResumeTime()
    {
        Time.timeScale = _timeScaleBeforePause;
    }
}
