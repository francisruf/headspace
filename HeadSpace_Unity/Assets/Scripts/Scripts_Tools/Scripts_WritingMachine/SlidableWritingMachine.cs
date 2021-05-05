using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WritingMachineController))]
public class SlidableWritingMachine : SlidableTool
{
    [Header("Other machine components")]
    public SpriteRenderer machineBGRenderer;
    private WritingMachineController _machineController;
    private WritingMachineKeyboard _keyboard;
    public ObjectInteractionZone openCloseInteractionZone0;
    public ObjectInteractionZone openCloseInteractionZone1;

    protected override void Awake()
    {
        base.Awake();
        _machineController = GetComponent<WritingMachineController>();
        _keyboard = GetComponentInChildren<WritingMachineKeyboard>();

        openCloseInteractionZone0.interactRequest += TriggerOpenClose;
        openCloseInteractionZone1.interactRequest += TriggerOpenClose;
    }

    private void OnDisable()
    {
        openCloseInteractionZone0.interactRequest -= TriggerOpenClose;
        openCloseInteractionZone1.interactRequest -= TriggerOpenClose;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerOpenClose(null);
            //_keyboard.PressKey(SpecialKey.Space);
        }
    }

    private void TriggerOpenClose(ObjectInteractionZone zone)
    {
        //Debug.Log("YO");

        if (IsOpen)
            TriggerAutoClose();
        else
            TriggerAutoOpen();
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
