using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Outbox : DropZone
{
    public static Action<List<MovableCommand>> newCommandRequest;
    private List<MovableCommand> _commandsInDropZone = new List<MovableCommand>();

    private void OnEnable()
    {
        CommandManager.commandRequestResult += OnCommandResult;
    }

    private void OnDisable()
    {
        CommandManager.commandRequestResult -= OnCommandResult;
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        base.AddObjectToDropZone(obj);

        MovableCommand cmd = obj.GetComponent<MovableCommand>();
        if (cmd != null)
        {
            _commandsInDropZone.Add(cmd);
        }
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);

        MovableCommand cmd = obj.GetComponent<MovableCommand>();
        if (cmd != null)
        {
            _commandsInDropZone.Remove(cmd);
        }
    }

    public void SendCommands()
    {
        Debug.Log("Sending commands...");

        if (newCommandRequest != null)
            newCommandRequest(_commandsInDropZone);
    }

    private void OnCommandResult(List<MovableCommand> commands)
    {
        bool success = true;

        foreach (var cmd in commands)
        {
            if (cmd.CurrentCommandState == CommandState.Sucess)
            {
                cmd.DisableObject();
            }
            else if (cmd.CurrentCommandState == CommandState.Fail)
            {
                success = false;
            }
        }
    }
}
