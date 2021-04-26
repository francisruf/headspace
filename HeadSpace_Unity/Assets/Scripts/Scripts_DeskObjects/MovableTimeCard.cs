using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableTimeCard : MovableObject
{
    [Header("Time card sprites")]
    public Sprite[] allSprites;

    protected override void Start()
    {
        base.Start();
        if (GameManager.instance != null)
            AssignSprite(GameManager.instance.CurrentDayInfo);
    }

    private void OnEnable()
    {
        GameManager.dayInfoChange += AssignSprite;
    }

    private void OnDisable()
    {
        GameManager.dayInfoChange -= AssignSprite;
    }

    private void AssignSprite(DayInfo info)
    {
        int spriteIndex = info.day;

        switch (info.time)
        {
            case LevelTime.DayStart:
                break;
            case LevelTime.NightEnd:
                transform.rotation = Quaternion.identity;
                spriteIndex += 2;
                break;

            case LevelTime.Level:
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                spriteIndex++;
                break;
            default:
                break;
        }

        if (spriteIndex < allSprites.Length)
            _spriteRenderer.sprite = allSprites[spriteIndex];
    }
}
