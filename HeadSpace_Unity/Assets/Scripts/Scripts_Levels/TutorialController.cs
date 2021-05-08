using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    public static Action shredderEnableRequest;
    public static Action openDrawerRequest;

    public GameObject endTrainingPrefab;
    public Transform endTrainingSpawnPos;
    public Transform endTrainingFinalPos;
    public Transform logbookSpawnPos;
    public Transform logbookFinalPos;

    [Header("Messages")]
    public List<TutorialMessage> allMessages;
    private int _messageCount;
    private MessageManager _messageManager;
    private MovableLogbook _logbook;

    // MILESTONES
    private bool _step_statusInit;
    private bool _step_statusComplete;
    private bool _step_statusTear;
    private bool _step_workerOrderPrint;
    private bool _step_shipMoveReceived;
    private bool _step_contractAssigned;
    private bool _step_clientEmbarked;
    private bool _step_contractComplete;
    private bool _step_writingMachineOpen;
    private bool _step_shredder;
    private bool _step_journalTrigger;
    private bool _step_journalTear;

    private void OnEnable()
    {
        MovableMessage_Special.specialMessageTeared += OnSpecialMessageTear;
        GameManager.levelStarted += OnLevelStart;
        Client.clientEmbarked += OnClientEmbarked;
        Contract.contractComplete += OnContractComplete;
        MovableContract.contractAssigned += OnContractAssigned;
        CommandManager.commandRequestResult += OnCommandResult;
        WritingMachineController.writingMachineOpen += OnMachineOpen;
        Receiver.specialMessagePrint += OnSpecialMessagePrint;
        MovableLogbook.logbookInitialized += OnLogbookInit;
    }

    private void OnDisable()
    {
        MovableMessage_Special.specialMessageTeared -= OnSpecialMessageTear;
        GameManager.levelStarted -= OnLevelStart;
        Client.clientEmbarked -= OnClientEmbarked;
        Contract.contractComplete -= OnContractComplete;
        MovableContract.contractAssigned -= OnContractAssigned;
        CommandManager.commandRequestResult -= OnCommandResult;
        WritingMachineController.writingMachineOpen -= OnMachineOpen;
        Receiver.specialMessagePrint -= OnSpecialMessagePrint;
        MovableLogbook.logbookInitialized -= OnLogbookInit;
    }

    private void Awake()
    {
        _messageCount = allMessages.Count;
    }

    private void Start()
    {
        _messageManager = MessageManager.instance;
        _logbook = FindObjectOfType<MovableLogbook>();

        if (_logbook != null)
            _logbook.transform.position = logbookSpawnPos.position;
    }

    private void OnLogbookInit(MovableLogbook logbook)
    {
        if (_logbook != null)
            return;
        this._logbook = logbook;
        logbook.transform.position = logbookSpawnPos.position;
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

        StartCoroutine(SpawnEndTraining());
    }

    private IEnumerator SpawnEndTraining()
    {
        yield return new WaitForSeconds(2f);
        MovableCommand_Keyword cmd = Instantiate(endTrainingPrefab, endTrainingSpawnPos.position, Quaternion.identity).GetComponent<MovableCommand_Keyword>();
        yield return AnimateNewObject(cmd, endTrainingSpawnPos.position, endTrainingFinalPos.position, true);
        Debug.Log("YO");
    }

    private void OnSpecialMessageTear(string messageName)
    {
        if (messageName == "MSG_Objectives")
        {
            NewMessage("MSG_ShipInfo");
        }
        else if (messageName == "MSG_ShipInfo")
        {
            StartCoroutine(StatusMessageTimer());
        }
        else if (messageName == "CMD_Status")
        {
            OnStatusTear();
        }
        else if (messageName == "MSG_WorkOrderBase")
        {
            StartCoroutine(WorkOrderBoardTimer());
        }
        else if (messageName == "MSG_WorkOrderDay")
        {
            StartCoroutine(EndTrainingMessageTimer());
        }
    }

    private void OnStatusTear()
    {
        if (!_step_statusComplete)
        {
            NewMessage("MSG_WorkOrderBase");
            _step_statusComplete = true;
        }
    }

    private void OnContractAssigned(MovableContract contract)
    {
        if (_step_contractAssigned)
            return;

        _step_contractAssigned = true;
        NewMessage("MSG_ShipMovement");
    }

    private void OnMoveReceived()
    {
        NewMessage("MSG_MoveReceived");
        StartCoroutine(EnableShredderTimer());
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

    private void OnSpecialMessagePrint(string messageName)
    {
        if (messageName == "MSG_Shredder" && !_step_shredder)
        {
            _step_shredder = true;
            if (shredderEnableRequest != null)
                shredderEnableRequest();
        }

        if (messageName == "MSG_WorkOrderBase" && !_step_workerOrderPrint)
        {
            _step_workerOrderPrint = true;

            if (ContractManager.instance != null)
                ContractManager.instance.TriggerNextContract();
        }

        if (messageName == "MSG_Journal" && !_step_journalTear)
        {
            if (_logbook != null)
            {
                _step_journalTear = true;
                StartCoroutine(AnimateNewObject(_logbook, logbookSpawnPos.position, logbookFinalPos.position, false));
            }
        }

        if (messageName == "MSG_EndTraining0")
        {
            if (ContractManager.instance != null)
                ContractManager.instance.ChangeContractConditions(ContractSpawnCondition.Timed, true);
        }

        if (messageName == "MSG_Markers")
        {
            if (openDrawerRequest != null)
                openDrawerRequest();
        }
    }

    private IEnumerator StatusMessageTimer()
    {
        if (_step_statusComplete)
            yield return null;
        else
        {
            yield return new WaitForSeconds(1.0f);
            NewMessage("MSG_ShipStatus");
            _step_statusInit = true;
        }
    }

    private IEnumerator WorkOrderBoardTimer()
    {
        yield return new WaitForSeconds(1.0f);
        NewMessage("MSG_WorkOrderBoard0");
        NewMessage("MSG_WorkOrderBoard1");
    }

    private IEnumerator EndTrainingMessageTimer()
    {
        yield return new WaitForSeconds(1.5f);
        NewMessage("MSG_EndTraining0");
        NewMessage("MSG_EndTraining1");
    }

    private IEnumerator EnableShredderTimer()
    {
        yield return new WaitForSeconds(4f);
        NewMessage("MSG_Shredder");
    }

    private IEnumerator EnableJournalTimer()
    {
        yield return new WaitForSeconds(2.5f);
        NewMessage("MSG_Journal");
    }


    private void OnCommandResult(List<MovableCommand> commands)
    {
        foreach (var cmd in commands)
        {
            if (cmd.CommandName.ToLower() == "move")
                if (cmd.CurrentCommandState == CommandState.Sucess)
                {
                    if (!_step_shipMoveReceived)
                    {
                        _step_shipMoveReceived = true;
                        OnMoveReceived();
                    }

                    if (_step_clientEmbarked && !_step_journalTrigger)
                    {
                        StartCoroutine(EnableJournalTimer());
                        _step_journalTrigger = true;
                    }
                }
            else if (cmd.CommandName.ToLower() == "stat")
                if (cmd.CurrentCommandState == CommandState.Sucess)
                {
                    _step_statusComplete = true;
                }
        }
    }

    private void OnMachineOpen(WritingMachineController controller)
    {
        if (_step_writingMachineOpen)
            return;

        MovableCommand cmd = controller.CurrentCommandDocument;
        if (cmd != null)
            if (cmd.CurrentCommandState == CommandState.Unsent)
            {
                string instructions = "<color=red><i>TEAR THIS DOCUMENT</color></i>";
                controller.PrintText(instructions, true, false);
                instructions = "- Compu-Typer<sup>tm</sup> USER GUIDE -";
                controller.PrintText(instructions, true, false);
                instructions = "1- Type in ship 3-letter CODE.";
                controller.PrintText(instructions, true, false);
                instructions = "2- Press ENTER to confirm.";
                controller.PrintText(instructions, true, false);
                instructions = "3- Type in command keyword.";
                controller.PrintText(instructions, true, false);
                instructions = "4- Confirm and tear when ready.";
                controller.PrintText(instructions, true, false);

                _step_writingMachineOpen = true;
            }
    }

    private void EndTutorial()
    {

    }

    private IEnumerator AnimateNewObject(InteractableObject obj, Vector2 startPos, Vector2 endPos, bool toggleInteractions)
    {
        if (toggleInteractions)
            obj.ToggleInteractions(false);

        float smoothTime = 0.2f;
        Vector2 velocity = new Vector2();

        while (Vector2.Distance(obj.transform.position, endPos) > 0.01f)
        {
            Vector2 smooth = Vector2.SmoothDamp(obj.transform.position, endPos, ref velocity, smoothTime);
            obj.transform.position = smooth;

            if (obj.IsSelected)
                break;

            yield return new WaitForEndOfFrame();
        }
        obj.transform.position = endPos;

        if (toggleInteractions)
            obj.ToggleInteractions(true);
    }
}


[System.Serializable]
public struct TutorialMessage
{
    public string messageName;
    [TextArea]
    public string messageText;
}
