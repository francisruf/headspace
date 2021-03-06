﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Outbox : DropZone
{
    public static Action<List<MovableCommand>> newCommandRequest;
    public static Action<MovableCommand> newSingleCommandRequest;

    public static Action commandSuccess;
    public static Action commandFail;
    public static Action timeCardSent;

    private List<MovableCommand> _commandsInDropZone = new List<MovableCommand>();
    private List<MovableCommand> _sentCommands = new List<MovableCommand>();
    private MovableTimeCard _timeCard;

    private SlidableOutbox _slidableOutbox;

    private Animator _animator;

    public List<string> startCommandsAccepted;

    private void OnEnable()
    {
        CommandManager.commandRequestResult += OnCommandResult;
        ObjectPlacer.newObjectInOutbox += OnNewObjectInOutbox;
    }

    private void OnDisable()
    {
        CommandManager.commandRequestResult -= OnCommandResult;
        ObjectPlacer.newObjectInOutbox -= OnNewObjectInOutbox;
    }

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInParent<Animator>();
        _slidableOutbox = GetComponentInParent<SlidableOutbox>();
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        Bounds objBounds = obj.ColliderBounds;
        Vector2 centerOffset = objBounds.center - obj.transform.position;

        float minX = _collider.bounds.min.x + (objBounds.size.x / 2f) - centerOffset.x;
        float maxX = _collider.bounds.max.x - (objBounds.size.x / 2f) - centerOffset.x;
        float minY = _collider.bounds.min.y + (objBounds.size.y / 2f) - centerOffset.y;
        float maxY = _collider.bounds.max.y - (objBounds.size.y / 2f) - centerOffset.y;

        Vector2 newPos = obj.transform.position;
        newPos.y -= 0.25f;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        obj.transform.position = newPos;
        obj.transform.parent = this.transform;

        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(HighestSortingOrder);
        HighestSortingOrder = obj.GetHighestOrder() + 1;

        //if (obj.Rigidbody != null)
        //{
        //    obj.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        //}

        MovableCommand cmd = obj.GetComponent<MovableCommand>();
        if (cmd != null)
        {
            _commandsInDropZone.Add(cmd);
        }

        if (!GameManager.GameStarted)
        {
            MovableTimeCard tc = obj.GetComponent<MovableTimeCard>();
            if (tc != null)
                _timeCard = tc;
        }
        _objectCount++;
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);

        MovableCommand cmd = obj.GetComponent<MovableCommand>();
        if (cmd != null)
        {
            _commandsInDropZone.Remove(cmd);
        }

        if (_timeCard != null)
        {
            if (_timeCard == obj.GetComponent<MovableTimeCard>())
                _timeCard = null;
        }
    }

    public override bool CheckIfAccepted(MovableObject obj)
    {
        if (_slidableOutbox != null)
            if (_slidableOutbox.IsFullyClosed)
                return false;

        return base.CheckIfAccepted(obj);
    }

    public void SendCommands()
    {
        //Debug.Log("Sending commands...");
        bool found = false;
        MovableCommand startCommand = null;

        foreach (var cmd in _commandsInDropZone)
        {
            foreach (var validName in startCommandsAccepted)
            {
                if (cmd.CommandName == validName)
                {
                    found = true;
                    startCommand = cmd;
                    break;
                }
            }
            if (found)
                break;
        }

        if (startCommand != null)
        {
            if (newSingleCommandRequest != null)
                newSingleCommandRequest(startCommand);

            return;
        }


        if (!GameManager.GameStarted)
        {
            if (_commandsInDropZone.Count > 0)
            {
                string error = "Procedure error : Please sign-in before sending any commands.";

                if (MessageManager.instance != null)
                    MessageManager.instance.GenericMessage(error, true);

                _animator.SetTrigger("RedLight");

                if (commandFail != null)
                    commandFail();
            }
        }
        else
        {
            if (newCommandRequest != null)
                newCommandRequest(new List<MovableCommand>(_commandsInDropZone));
        }
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

    private void OnNewObjectInOutbox()
    {
        _animator.SetTrigger("BlueLight");
    }
}
