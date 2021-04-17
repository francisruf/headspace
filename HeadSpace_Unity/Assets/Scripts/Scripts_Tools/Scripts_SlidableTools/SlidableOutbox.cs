using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableOutbox : SlidableTool
{
    public static Action outboxAutoOpen;

    [Header("Animation settings")]
    //public float smoothTime;

    private DropZone_Outbox _dropZone;
    private Animator _animator;

    //private Vector2 _lerpStartPos;
    //private IEnumerator _openingRoutine;

    private void OnEnable()
    {
        WritingMachineController.commandReadyToTear += OnCommandReadyToTear;
    }

    private void OnDisable()
    {
        WritingMachineController.commandReadyToTear -= OnCommandReadyToTear;
    }

    protected override void Awake()
    {
        base.Awake();
        _dropZone = GetComponentInChildren<DropZone_Outbox>();
        _animator = GetComponent<Animator>();
    }

    //protected override void Update()
    //{
    //    base.Update();

    //    if (Input.GetKeyDown(KeyCode.O))
    //    {
    //        StartCoroutine(OpenOutbox());
    //    }
    //}

    protected override void OpenTool()
    {
        base.OpenTool();
        if (drawerOpened != null)
            drawerOpened();
    }

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
        _dropZone.SendCommands();

        if (drawerClosed != null)
            drawerClosed();
    }

    private void OnCommandReadyToTear()
    {
        if (_openingRoutine != null)
            return;

        _openingRoutine = AutoOpenTool();
        StartCoroutine(_openingRoutine);
    }

    protected override IEnumerator AutoOpenTool()
    {
        if (outboxAutoOpen != null)
            outboxAutoOpen();

        //Debug.Log("DERIVED");

        yield return base.AutoOpenTool();
    }
}
