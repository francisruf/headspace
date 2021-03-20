using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SlidableReceptor : SlidableTool
{
    [Header("Receptor settings")]
    public GameObject messagePrefab;
    public Transform printPoint;

    private Animator _printerAnimator;
    public TextMeshProUGUI timeText;
    private MovableMessage _messageInSlot;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        _printerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (TimeManager.instance != null)
        {
            UpdateTime();
        }
    }

    private void OnEnable()
    {
        MessageManager.newMessageReceived += NewMessageReceived;
    }

    private void OnDisable()
    {
        MessageManager.newMessageReceived -= NewMessageReceived;
    }

    private void NewMessageReceived()
    {
        _printerAnimator.SetTrigger("NewMessage");
    }

    //Fonction qui vérifie si le récepteur peut imprimer le prochain message
    public bool CanPrintMessages()
    {
        if (!IsOpen)
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
