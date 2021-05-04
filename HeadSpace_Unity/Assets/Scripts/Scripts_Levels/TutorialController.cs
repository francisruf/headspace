using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public List<TutorialMessage> allMessages;
    private int _messageCount;
    private MessageManager _messageManager;

    // MILESTONES
    private bool _step_statusInit;
    private bool _step_statusComplete;
    private bool _step_shipMoveReceived;
    private bool _step_clientEmbarked;
    private bool _step_contractComplete;

    private void OnEnable()
    {
        MovableMessage_Special.specialMessageTeared += OnSpecialMessageTear;
        GameManager.levelStarted += OnLevelStart;
        Client.clientEmbarked += OnClientEmbarked;
        Contract.contractComplete += OnContractComplete;
        MovableContract.contractAssigned += OnContractAssigned;
        CommandManager.commandRequestResult += OnCommandResult;
    }

    private void OnDisable()
    {
        MovableMessage_Special.specialMessageTeared -= OnSpecialMessageTear;
        GameManager.levelStarted -= OnLevelStart;
        Client.clientEmbarked -= OnClientEmbarked;
        Contract.contractComplete -= OnContractComplete;
        MovableContract.contractAssigned -= OnContractAssigned;
        CommandManager.commandRequestResult -= OnCommandResult;
    }

    private void Awake()
    {
        _messageCount = allMessages.Count;
    }

    private void Start()
    {
        _messageManager = MessageManager.instance;
    }

    private void NewMessage(string messageName)
    {
        string messageText = "";
        bool found = false;
        for (int i = 0; i < _messageCount; i++)
        {
            if (messageName == allMessages[i].messageName)
            {
                messageText = allMessages[i].messageText;
                found = true;
                break;
            }
        }

        if (found)
        {
            _messageManager.SpecialMessage(messageName, messageText);
        }
        else
        {
            Debug.LogError("Warning - could not find tutorial message named : " + messageName);
        }
    }

    private void OnLoad()
    {
        // TODO : WritingMachine message
    }

    private void OnLevelStart()
    {
        NewMessage("MSG_Welcome");
        NewMessage("MSG_Objectives");
    }

    private void OnSpecialMessageTear(string messageName)
    {
        if (messageName == "MSG_Objectives")
        {
            NewMessage("MSG_ShipInfo");
            NewMessage("MSG_ShipStatus");
            _step_statusInit = true;
        }

        else if (messageName == "CMD_Status")
        {
            OnStatusTear();
        }
    }

    private void OnStatusTear()
    {
        if (!_step_statusInit)
            return;

        if (!_step_statusComplete)
        {
            NewMessage("MSG_WorkOrderBase");
            NewMessage("MSG_WorkOrderBoard");
            _step_statusComplete = true;
        }
    }

    private void OnContractAssigned(MovableContract contract)
    {
        NewMessage("MSG_ShipMovement");
    }

    private void OnMoveReceived()
    {
        NewMessage("MSG_MoveReceived");
    }

    private void OnClientEmbarked()
    {
        if (_step_clientEmbarked)
            return;

        _step_clientEmbarked = true;

        NewMessage("MSG_ClientPickedUp");
        NewMessage("MSG_Markers");
    }

    private void OnContractComplete(int mySuperInteger)
    {
        if (_step_contractComplete)
            return;

        _step_contractComplete = true;

        NewMessage("MSG_WorkOrderComplete");
        NewMessage("MSG_WorkOrderDay");
    }

    private void OnCommandResult(List<MovableCommand> commands)
    {
        if (_step_shipMoveReceived)
            return;

        foreach (var cmd in commands)
        {
            if (cmd.CommandName.ToLower() == "move")
                if (cmd.CurrentCommandState == CommandState.Sucess)
                {
                    _step_shipMoveReceived = true;
                    OnMoveReceived();
                }
        }
    }

}


[System.Serializable]
public struct TutorialMessage
{
    public string messageName;
    public string messageText;
}
