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
        MovableCommand_Keyword.openOutboxRequest += OnOpenRequest;
    }

    private void OnDisable()
    {
        WritingMachineController.commandReadyToTear -= OnCommandReadyToTear;
        MovableCommand_Keyword.openOutboxRequest -= OnOpenRequest;
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

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
        _dropZone.SendCommands();
    }

    private void OnOpenRequest()
    {
        OnCommandReadyToTear(true);
    }

    private void OnCommandReadyToTear(bool openDrawer)
    {
        if (!openDrawer)
            return;

        if (_openingRoutine != null)
            return;

        _openingRoutine = AutoOpenTool();
        StartCoroutine(_openingRoutine);
    }

    protected override IEnumerator AutoOpenTool()
    {
        if (!IsOpen)
        {
            //if (outboxAutoOpen != null)
            //    outboxAutoOpen();
        }
        //Debug.Log("DERIVED");

        yield return base.AutoOpenTool();
    }
}
