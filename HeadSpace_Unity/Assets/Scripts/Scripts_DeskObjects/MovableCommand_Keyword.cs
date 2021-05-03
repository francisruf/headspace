using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableCommand_Keyword : MovableCommand
{
    public string commandName;

    protected override void Awake()
    {
        base.Awake();
        CommandName = commandName;
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
