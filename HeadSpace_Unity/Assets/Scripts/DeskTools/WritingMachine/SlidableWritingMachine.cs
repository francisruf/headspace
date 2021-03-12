using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WritingMachineController))]
public class SlidableWritingMachine : SlidableTool
{
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
}
