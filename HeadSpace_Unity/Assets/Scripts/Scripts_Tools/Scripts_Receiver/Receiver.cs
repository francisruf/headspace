using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Receiver : StaticTool
{
    public static Action<bool> singlePrint;
    public static Action<string> specialMessagePrint;

    [Header("Receiver settings")]
    public GameObject messagePrefab;
    public GameObject specialMessagePrefab;
    public Transform printPoint;

    private Animator _printerAnimator;
    public TextMeshProUGUI timeText;
    private MovableMessage _messageInSlot;

    public List<string> indestructableMessageNames;

    protected override void Awake()
    {
        base.Awake();
        _isEnabled = true;
        _printerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        MessageManager.newMessageReceived += NewMessageReceived;
    }

    private void OnDisable()
    {
        MessageManager.newMessageReceived -= NewMessageReceived;
    }

    // Update is called once per frame
    private void Update()
    {
        if (TimeManager.instance != null)
            UpdateTime();
    }

    private void NewMessageReceived(bool playSound)
    {
        _printerAnimator.SetTrigger("NewMessage");
    }

    //Fonction qui vérifie si le récepteur peut imprimer le prochain message
    public bool CanPrintMessages()
    {
        if (!_isEnabled)
            return false;

        if (_messageInSlot != null)
            return false;

        return true;
    }

    //Si le recepteur est sorti: Print message (Print next message in Queue plus tard)

    public void PrintMessage(string messageText, Color messageColor)
    {
        // Ne pas imprimer de message si un message est dans la fente
        if (_messageInSlot != null)
            return;

        _messageInSlot = Instantiate(messagePrefab, printPoint).GetComponent<MovableMessage>();
        _messageInSlot.transform.localPosition = Vector2.zero;
        _messageInSlot.SetText(messageText, messageColor);
        _messageInSlot.SetSortingLayer(_spriteRenderer.sortingLayerID);
        _messageInSlot.SetOrderInLayer(_spriteRenderer.sortingOrder + 1);

        _printerAnimator.SetBool("MessageInSlot", true);
        _messageInSlot.messageTeared += ClearSlot;
    }

    public void PrintTutorialMessage(string messageName, string messageText, Color messageColor)
    {
        // Ne pas imprimer de message si un message est dans la fente
        if (_messageInSlot != null)
            return;

        MovableMessage_Special tutMessage = Instantiate(specialMessagePrefab, printPoint).GetComponent<MovableMessage_Special>();
        tutMessage.messageName = messageName;
        _messageInSlot = tutMessage;
        _messageInSlot.transform.localPosition = Vector2.zero;
        _messageInSlot.SetText(messageText, messageColor);
        _messageInSlot.SetSortingLayer(_spriteRenderer.sortingLayerID);
        _messageInSlot.SetOrderInLayer(_spriteRenderer.sortingOrder + 1);

        _printerAnimator.SetBool("MessageInSlot", true);
        _messageInSlot.messageTeared += ClearSlot;

        if (indestructableMessageNames.Contains(messageName))
            _messageInSlot.tag = "Indestruc";

        if (specialMessagePrint != null)
            specialMessagePrint(messageName);
    }

    public void MessageFullyPrinted()
    {
        if (_messageInSlot != null)
            _messageInSlot.EnableCollider();
    }

    private void ClearSlot()
    {
        _messageInSlot.transform.parent = null;
        _messageInSlot.messageTeared -= ClearSlot;
        _messageInSlot = null;
        _printerAnimator.SetBool("MessageInSlot", false);
    }

    private void UpdateTime()
    {
        string currentTime = TimeSpan.FromSeconds(TimeManager.instance.GameTime).ToString(@"hh\:mm");
        timeText.text = currentTime;
    }

    public void TriggerPrintSound(int lastPrint)
    {
        bool lp = lastPrint == 1 ? true : false;

        if (singlePrint != null)
            singlePrint(lp);
    }
}
