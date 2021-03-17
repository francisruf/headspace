using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Outbox : DropZone
{
    public static Action<List<MovableCommand>> newCommandRequest;

    public static Action commandSuccess;
    public static Action commandFail;

    private List<MovableCommand> _commandsInDropZone = new List<MovableCommand>();
    private List<MovableCommand> _sentCommands = new List<MovableCommand>();

    private Animator _animator;

    private void OnEnable()
    {
        CommandManager.commandRequestResult += OnCommandResult;
    }

    private void OnDisable()
    {
        CommandManager.commandRequestResult -= OnCommandResult;
    }

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInParent<Animator>();
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
        //Debug.Log("Sending commands...");

        if (newCommandRequest != null)
            newCommandRequest(_commandsInDropZone);
    }

    private void OnCommandResult(List<MovableCommand> commands)
    {
        bool commandsFailed = false;
        bool commandsSent = false;


        foreach (var cmd in commands)
        {
            if (cmd.CurrentCommandState == CommandState.Sucess)
            {
                cmd.DisableObject();
                commandsSent = true;
                _sentCommands.Add(cmd);
            }
            else if (cmd.CurrentCommandState == CommandState.Fail)
            {
                commandsFailed = true;
            }
        }

        foreach (var cmd in _sentCommands)
        {
            _commandsInDropZone.Remove(cmd);
        }
        _sentCommands.Clear();

        if (commandsSent)
        {
            if (commandsFailed)
            {
                _animator.SetTrigger("GreenRedLight");

                if (commandFail != null)
                    commandFail();
            }
            else
            {
                _animator.SetTrigger("GreenLight");

                if (commandSuccess != null)
                    commandSuccess();
            }
        }
        else if (commandsFailed)
        {
            _animator.SetTrigger("RedLight");

            if (commandFail != null)
                commandFail();
        }
    }
}
