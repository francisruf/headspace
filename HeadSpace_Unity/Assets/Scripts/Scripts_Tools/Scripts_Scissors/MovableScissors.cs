using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableScissors : MovableObject
{
    public static Action scissorsCut;
    private bool _isOpen;

    [Header("Scissors settings")]
    public Sprite openSprite;
    public Sprite closedSprite;

    protected override void Update()
    {
        base.Update();
        if (IsSelected && Input.GetMouseButtonDown(1))
        {
            CloseScissors();
        }

        if (!_isOpen && !Input.GetMouseButton(1))
        {
            OpenScissors();
        }
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);
        OpenScissors();
    }

    private void CloseScissors()
    {
        _isOpen = false;
        _spriteRenderer.sprite = closedSprite;
        _shadowRenderer.sprite = closedSprite;
    }

    private void OpenScissors()
    {
        _isOpen = true;
        _spriteRenderer.sprite = openSprite;
        _shadowRenderer.sprite = openSprite;

        if (scissorsCut != null)
            scissorsCut();
    }

}
