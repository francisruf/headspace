using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineClock : MonoBehaviour
{
    public static Action clockTick;

    private SpriteRenderer _spriteRenderer;

    public GameObject digitPrefab;
    public Transform[] digitPositions;
    private List<SpriteRenderer> _digitRenderers = new List<SpriteRenderer>();
    private WritingMachineSpriteDB _spriteDB;

    public Sprite regularSprite;
    public Sprite yellowSprite;

    private bool _flash;
    private bool _nearEnd;
    private int _previousHours;
    private int _previousMinutes;

    private IEnumerator _timeRoutine;

    private void OnEnable()
    {
        GameManager.levelEnded += OnLevelEnded;
        TimeManager.timeSet += AssignTime;
    }

    private void OnDisable()
    {
        GameManager.levelEnded -= OnLevelEnded;
        TimeManager.timeSet -= AssignTime;
    }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteDB = GetComponentInParent<WritingMachineSpriteDB>();
        foreach (var pos in digitPositions)
        {
            SpriteRenderer sr = Instantiate(digitPrefab, pos.position, Quaternion.identity, pos).GetComponent<SpriteRenderer>();
            _digitRenderers.Add(sr);
        }
    }

    private void AssignTime(bool timeStarted)
    {
        if (timeStarted)
        {
            if (_timeRoutine == null)
            {
                _timeRoutine = SetTime();
                StartCoroutine(SetTime());
            }
        }
        else
        {
            if (_timeRoutine != null)
            {
                StopCoroutine(_timeRoutine);
                _timeRoutine = null;
            }

            foreach (var sr in _digitRenderers)
            {
                sr.enabled = false;
            }
        }
    }

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

                    _spriteRenderer.sprite = regularSprite;
                    _digitRenderers[0].sprite = _spriteDB.GetDigit(_previousHours / 10, true);
                    _digitRenderers[1].sprite = _spriteDB.GetDigit(_previousHours % 10, true);
                    _digitRenderers[2].sprite = _spriteDB.GetDigit(_previousMinutes / 10, true);
                    _digitRenderers[3].sprite = _spriteDB.GetDigit(_previousMinutes % 10, true);
                }
                else
                {
                    if (clockTick != null)
                        clockTick();

                    _spriteRenderer.sprite = yellowSprite;
                    _digitRenderers[0].sprite = _spriteDB.GetDigit(_previousHours / 10, false);
                    _digitRenderers[1].sprite = _spriteDB.GetDigit(_previousHours % 10, false);
                    _digitRenderers[2].sprite = _spriteDB.GetDigit(_previousMinutes / 10, false);
                    _digitRenderers[3].sprite = _spriteDB.GetDigit(_previousMinutes % 10, false);

                    yield return new WaitForSeconds(0.4f);

                    _spriteRenderer.sprite = regularSprite;
                    _digitRenderers[0].sprite = _spriteDB.GetDigit(_previousHours / 10, true);
                    _digitRenderers[1].sprite = _spriteDB.GetDigit(_previousHours % 10, true);
                    _digitRenderers[2].sprite = _spriteDB.GetDigit(_previousMinutes / 10, true);
                    _digitRenderers[3].sprite = _spriteDB.GetDigit(_previousMinutes % 10, true);
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnLevelEnded()
    {
        if (_timeRoutine != null)
        {
            StopCoroutine(_timeRoutine);
            _timeRoutine = null;
        }
    }
}
