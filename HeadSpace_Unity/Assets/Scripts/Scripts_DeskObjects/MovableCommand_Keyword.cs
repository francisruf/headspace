using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCommand_Keyword : MovableCommand
{
    public static Action openOutboxRequest;

    public string commandName;
    public bool startTeared = false;
    public bool openOutboxOnSelect;


    protected override void Awake()
    {
        base.Awake();
        CommandName = commandName;
        _wasTeared = startTeared;
    }

    protected override void AssignSpriteStartSize()
    {
        UpdateVisuals();
    }

    protected override void UpdateVisuals()
    {
        base.UpdateVisuals();
    }

    public override void Select(bool fireEvent = true)
    {
        base.Select(fireEvent);

        if (openOutboxRequest != null)
            openOutboxRequest();

    }
}
