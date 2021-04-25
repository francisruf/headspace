using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineClock : MonoBehaviour
{
    public GameObject digitPrefab;
    public Transform[] digitPositions;
    private List<SpriteRenderer> _digitRenderers = new List<SpriteRenderer>();
    private WritingMachineSpriteDB _spriteDB;

    private bool _flash;
    private bool _nearEnd;
    private int _previousHours;
    private int _previousMinutes;

    private IEnumerator _timeRoutine;

    private void OnEnable()
    {
        GameManager.levelEnded += OnLevelEnded;
    }

    private void OnDisable()
    {
        GameManager.levelEnded -= OnLevelEnded;
    }

    private void Awake()
    {
        _spriteDB = GetComponentInParent<WritingMachineSpriteDB>();
        foreach (var pos in digitPositions)
        {
            SpriteRenderer sr = Instantiate(digitPrefab, pos.position, Quaternion.identity, pos).GetComponent<SpriteRenderer>();
            _digitRenderers.Add(sr);
        }

        _timeRoutine = SetTime();
        StartCoroutine(SetTime());
    }


    //private void SetTime(float time)
    //{
    //    float time = 0f;

    //    if (TimeManager.instance != null)
    //        time = TimeManager.instance.GetCurrentTime;


    //    int hours = TimeSpan.FromSeconds(time).Hours;
    //    int minutes = TimeSpan.FromSeconds(time).Minutes;

    //    if (hours != _previousHours || minutes != _previousMinutes)
    //    {
    //        _previousHours = hours;
    //        _previousMinutes = minutes;

    //        _digitRenderers[0].sprite = _spriteDB.GetDigit(_previousHours / 10);
    //        _digitRenderers[1].sprite = _spriteDB.GetDigit(_previousHours % 10);
    //        _digitRenderers[2].sprite = _spriteDB.GetDigit(_previousMinutes / 10);
    //        _digitRenderers[3].sprite = _spriteDB.GetDigit(_previousMinutes % 10);

    //        if (!_nearEnd)
    //        {
    //            if (TimeManager.instance != null)
    //                _nearEnd = _previousHours >= TimeManager.instance.hurryUpHours ? true : false;

    //            if (_nearEnd)
    //            {
    //                _timeRoutine = FlashDigits();
    //                StartCoroutine(_timeRoutine);
    //            }
    //        }
    //        else
    //        {
    //            _flash = true;
    //        }
    //    }
    //}

    private IEnumerator SetTime()
    {
        float time = 0f;

        while (true)
        {
            if (TimeManager.instance != null)
                time = TimeManager.instance.GetCurrentTime;

            int hours = TimeSpan.FromSeconds(time).Hours;
            int minutes = TimeSpan.FromSeconds(time).Minutes;

            if (hours != _previousHours || minutes != _previousMinutes)
            {
                _previousHours = hours;
                _previousMinutes = minutes;

                if (!_nearEnd)
                {
                    if (TimeManager.instance != null)
                        _nearEnd = _previousHours >= TimeManager.instance.hurryUpHours ? true : false;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        _digitRenderers[i].enabled = false;
                    }
                }
                yield return new WaitForSeconds(0.2f);
                
                _digitRenderers[0].sprite = _spriteDB.GetDigit(_previousHours / 10);
                _digitRenderers[1].sprite = _spriteDB.GetDigit(_previousHours % 10);
                _digitRenderers[2].sprite = _spriteDB.GetDigit(_previousMinutes / 10);
                _digitRenderers[3].sprite = _spriteDB.GetDigit(_previousMinutes % 10);

                for (int i = 0; i < 4; i++)
                {
                    _digitRenderers[i].enabled = true;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnLevelEnded()
    {
        StopCoroutine(_timeRoutine);
        _timeRoutine = null;
    }
}
