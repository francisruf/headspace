using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogbookPage_Simple : LogbookPage
{
    [Header("Sector page settings")]
    public GameObject pageParent;

    public override void DisplayPage(out Sprite newSprite)
    {
        base.DisplayPage(out newSprite);
        pageParent.SetActive(true);
    }

    public override void HidePage()
    {
        base.HidePage();
        pageParent.SetActive(false);
    }
}
