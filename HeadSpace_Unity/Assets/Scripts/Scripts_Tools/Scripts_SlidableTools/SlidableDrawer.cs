using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableDrawer : SlidableTool
{
    public DrawerType drawerType;

    private void OnEnable()
    {
        TutorialController.openDrawerRequest += OnOpenDrawerRequest;
    }

    private void OnDisable()
    {
        TutorialController.openDrawerRequest -= OnOpenDrawerRequest;
    }

    private void OnOpenDrawerRequest()
    {
        TriggerAutoOpen();
    }

    protected override void OpenTool()
    {
        base.OpenTool();
    }

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
    }
}

public enum DrawerType
{
    Solo,
    Double
}
