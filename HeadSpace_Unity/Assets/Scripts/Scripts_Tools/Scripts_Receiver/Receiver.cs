using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Receiver : StaticTool
{
    [Header("Receiver settings")]
    public GameObject messagePrefab;
    public Transform printPoint;

    private Animator _printerAnimator;
    public TextMeshProUGUI timeText;
    private MovableMessage _messageInSlot;


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

    private void NewMessageReceived()
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

    public void PrintMessage(string messageText)
    {
        // Ne pas imprimer de message si un message est dans la fente
        if (_messageInSlot != null)
            return;

        _messageInSlot = Instantiate(messagePrefab, printPoint).GetComponent<MovableMessage>();
        _messageInSlot.transform.localPosition = Vector2.zero;
        _messageInSlot.SetText(messageText);

        _printerAnimator.SetBool("MessageInSlot", true);
        _messageInSlot.messageTeared += ClearSlot;
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

}
