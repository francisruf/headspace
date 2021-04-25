using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromoTracker : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public Sprite[] daySprites;
    public int startRemainingDays;

    private int remainingDays;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateRemainingDays();
    }

    private void UpdateRemainingDays()
    {
        remainingDays = Mathf.Clamp(startRemainingDays - GameManager.instance.CurrentDayInfo.day, 0, int.MaxValue);
        
        if (remainingDays < daySprites.Length)
        {
            _spriteRenderer.sprite = daySprites[remainingDays];
        }
    }


}
