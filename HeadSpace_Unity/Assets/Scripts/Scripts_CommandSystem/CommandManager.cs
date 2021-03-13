using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    // Liste de toute les commandes, remplie avec les commands qui se trouvent sur le même gameObject
    private List<Command> _allCommands = new List<Command>();

    public string testCommandName;
    public string testShipName;
    public string testCoordinates;

    private void OnEnable()
    {
        CommandDebugWindow.newCommandRequest += OnNewCommandRequest;
    }

    private void OnDisable()
    {
        CommandDebugWindow.newCommandRequest -= OnNewCommandRequest;
    }

    private void Awake()
    {
        foreach (var cmd in GetComponents<Command>())
        {
            _allCommands.Add(cmd);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            OnNewCommandRequest(testCommandName, testShipName, testCoordinates);
        }
    }

    private void OnNewCommandRequest(string commandName, string shipName, string coordinates)
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
            bool commandIsValid = foundCommand.TryExecution(shipName, coordinates, out errorMessage);

            if (!commandIsValid)
            {
                Debug.Log("COMMAND MANAGER - " + errorMessage);
            }
        }

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

        return foundCommand;
    }

}
