using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : StaticTool
{
    private ShredderSlot child = null;

    private void OnEnable()
    {
        TutorialController.shredderEnableRequest += OnShredderEnableRequest;
    }

    private void OnDisable()
    {
        TutorialController.shredderEnableRequest -= OnShredderEnableRequest;
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        child = transform.GetComponentInChildren<ShredderSlot>();
    }

    protected override void Start()
    {
        base.Start();
        child.canShred = startEnabled;
        child.UpdateLightState();
    }

    private void OnShredderEnableRequest()
    {
        ToggleInteractions(true);
        child.TriggerLightsEnable();
    }

    public override void ToggleInteractions(bool toggleON)
    {
        _interactionsEnabled = toggleON;
        child.canShred = true;
        child.UpdateLightState();
    }

    public override void DisableObject()
    {
        base.DisableObject();
        Debug.Log("AS:DLK");
    }
}
