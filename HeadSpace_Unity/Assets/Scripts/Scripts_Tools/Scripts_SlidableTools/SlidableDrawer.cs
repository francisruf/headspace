using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableDrawer : SlidableTool
{
    protected override void OpenTool()
    {
        base.OpenTool();
        if (drawerOpened != null)
            drawerOpened();
    }

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
        if (drawerClosed != null)
            drawerClosed();
    }
}
