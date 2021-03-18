using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogbookPage : MonoBehaviour
{
    [Header("General page settings")]
    public Sprite pageSprite;

    public virtual void DisplayPage(out Sprite newSprite)
    {
        newSprite = pageSprite;
    }

    public virtual void HidePage()
    {

    }
}
