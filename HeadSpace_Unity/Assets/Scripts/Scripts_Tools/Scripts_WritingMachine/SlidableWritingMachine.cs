using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WritingMachineController))]
public class SlidableWritingMachine : SlidableTool
{
    public SpriteRenderer machineBGRenderer;
    private WritingMachineController _machineController;

    protected override void Awake()
    {
        base.Awake();
        _machineController = GetComponent<WritingMachineController>();
    }

    protected override void OpenTool()
    {
        base.OpenTool();
        _machineController.OnMachineOpen();
    }

    protected override void CloseTool()
    {
        base.CloseTool();
        _machineController.OnMachineClose();
    }

    public override int GetSortingLayer()
    { 
        if (machineBGRenderer != null)
        {
            return SortingLayer.GetLayerValueFromID(machineBGRenderer.sortingLayerID);
        }

        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
            return 0;
        }
    }

    public override int GetOrderInLayer()
    {
        if (machineBGRenderer != null)
        {
            return machineBGRenderer.sortingOrder;
        }
        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
            return 0;
        }
    }

    //public override void 
}
