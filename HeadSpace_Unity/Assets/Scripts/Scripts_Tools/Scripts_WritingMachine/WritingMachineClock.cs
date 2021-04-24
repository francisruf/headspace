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

    private void Awake()
    {
        _spriteDB = GetComponentInParent<WritingMachineSpriteDB>();
        foreach (var pos in digitPositions)
        {
            SpriteRenderer sr = Instantiate(digitPrefab, pos.position, Quaternion.identity, pos).GetComponent<SpriteRenderer>();
            _digitRenderers.Add(sr);
        }
    }

    private void Update()
    {
        float time = 0f;

        if (TimeManager.instance != null)
            time = TimeManager.instance.GetCurrentTime;

        SetTime(time);
    }

    private void SetTime(float time)
    {
        int hours = TimeSpan.FromSeconds(time).Hours;
        int minutes = TimeSpan.FromSeconds(time).Minutes;

        //Debug.Log("HOURS : " + hours);
        //Debug.Log("MINS : " + minutes);
        //Debug.Log((8 / 10).ToString());
        _digitRenderers[0].sprite = _spriteDB.GetDigit(hours / 10);
        _digitRenderers[1].sprite = _spriteDB.GetDigit(hours % 10);
        _digitRenderers[2].sprite = _spriteDB.GetDigit(minutes / 10);
        _digitRenderers[3].sprite = _spriteDB.GetDigit(minutes % 10);
    }
}
