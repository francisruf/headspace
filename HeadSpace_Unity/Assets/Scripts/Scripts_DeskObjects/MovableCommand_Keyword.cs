using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCommand_Keyword : MovableCommand
{
    public string commandName;
    public bool startTeared = false;

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
}
