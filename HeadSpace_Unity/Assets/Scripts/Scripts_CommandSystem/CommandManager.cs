using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public static Action<List<MovableCommand>> commandRequestResult; 

    // Liste de toute les commandes, remplie avec les commands qui se trouvent sur le même gameObject
    private List<Command> _allCommands = new List<Command>();

    private void OnEnable()
    {
        CommandDebugWindow.newCommandRequest += OnNewCommandRequest;
        DropZone_Outbox.newCommandRequest += OnNewCommandRequest;
        DropZone_Outbox.newSingleCommandRequest += OnNewCommandRequest;
    }

    private void OnDisable()
    {
        CommandDebugWindow.newCommandRequest -= OnNewCommandRequest;
        DropZone_Outbox.newCommandRequest -= OnNewCommandRequest;
        DropZone_Outbox.newSingleCommandRequest -= OnNewCommandRequest;
    }

    private void Awake()
    {
        foreach (var cmd in GetComponents<Command>())
        {
            _allCommands.Add(cmd);
        }
    }

    private void OnNewCommandRequest(MovableCommand command)
    {

            Command foundCommand = FindCommand(command.CommandName);
            bool commandIsValid = false;

            if (foundCommand == null)
            {
                Debug.Log("COMMAND MANAGER - Invalid command name : " + command.CommandName);
                commandIsValid = false;
            }

            else
            {
                string errorMessage = "";
                if (command.Route != null)
                    commandIsValid = foundCommand.TryExecution(command.ShipCallsign, command.Route, out errorMessage);
                else
                    commandIsValid = foundCommand.TryExecution(command.ShipCallsign, command.TargetGridCoords, command.ProductCode, out errorMessage);
            }

            if (commandIsValid)
                command.CurrentCommandState = CommandState.Sucess;
            else
                command.CurrentCommandState = CommandState.Fail;

        List<MovableCommand> result = new List<MovableCommand>();
        result.Add(command);

        if (commandRequestResult != null)
            commandRequestResult(result);
    }

    private void OnNewCommandRequest(List<MovableCommand> commands)
    {
        foreach (var cmd in commands)
        {
            Command foundCommand = FindCommand(cmd.CommandName);
            bool commandIsValid = false;

            if (foundCommand == null)
            {
                Debug.Log("COMMAND MANAGER - Invalid command name : " + cmd.CommandName);
                commandIsValid = false;
            }

            else
            {
                string errorMessage = "";
                if (cmd.Route != null)
                    commandIsValid = foundCommand.TryExecution(cmd.ShipCallsign, cmd.Route, out errorMessage);
                else
                    commandIsValid = foundCommand.TryExecution(cmd.ShipCallsign, cmd.TargetGridCoords, cmd.ProductCode, out errorMessage);
            }

            if (commandIsValid)
                cmd.CurrentCommandState = CommandState.Sucess;
            else
                cmd.CurrentCommandState = CommandState.Fail;
        }

        if (commandRequestResult != null)
            commandRequestResult(commands);
    }

    private void OnNewCommandRequest(string commandName, string shipName, string coordinates, string productCode)
    {
        Command foundCommand = FindCommand(commandName);

        if (foundCommand == null)
        {
            Debug.Log("COMMAND MANAGER - Invalid command name : " + commandName);
            return;
        }

        if (foundCommand != null)
        {
            string errorMessage = "";
            bool commandIsValid = foundCommand.TryExecution(shipName, coordinates, productCode, out errorMessage);

            if (!commandIsValid)
            {
                Debug.Log("COMMAND MANAGER - " + errorMessage);
            }
        }
        Debug.Log("3");
    }

    private Command FindCommand(string candidateKeyword)
    {
        candidateKeyword = candidateKeyword.ToUpper();
        Command foundCommand = null;

        foreach (var cmd in _allCommands)
        {
            if (cmd.keyWord.ToUpper() == candidateKeyword)
            {
                foundCommand = cmd;
                break;
            }
        }

        if (foundCommand == null)
        {
            string errorMessage = "";
            if (candidateKeyword.Length > 0)
            {
                errorMessage = "Syntax error : \n\n";
                errorMessage += "Command " + candidateKeyword + " does not exist.";
            }
            else
            {
                errorMessage = "Syntax error : \n\n";
                errorMessage += "No command entered.";
            }

            if (MessageManager.instance != null)
                MessageManager.instance.GenericMessage(errorMessage, true);
        }

        return foundCommand;
    }

}
