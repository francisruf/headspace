using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogbookPage : MonoBehaviour
{
    public MovableLogbook Logbook { get; set; }

    [Header("General page settings")]
    public Sprite pageSprite;

    public virtual void InitializePage(MovableLogbook logbook)
    {
        Logbook = logbook;
    }

    public virtual void DisplayPage(out Sprite newSprite)
    {
        newSprite = pageSprite;
    }

    public virtual void HidePage()
    {

    }
}
