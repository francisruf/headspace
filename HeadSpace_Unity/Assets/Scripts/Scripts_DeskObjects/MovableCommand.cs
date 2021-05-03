using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovableCommand : MovableObject
{
    public Action commandTeared;
    public static Action commandSinglePrint;
    public static Action commandInOutbox;

    private TextMeshProUGUI _commandText;
    private Canvas _canvas;
    private BoxCollider2D _boxCollider2D;

    public float lineHeight;
    private bool _wasTeared;

    private float _previousSpriteHeight;

    public string CommandName { get; protected set; } = "";
    public string ShipName { get; private set; } = "";
    public string TargetGridCoords { get; private set; } = "";
    public string ProductCode { get; private set; } = "";
    public List<string> Route { get; private set; }

    private Queue<string> _printingQueue = new Queue<string>();
    private int _printQueueCount;
    private IEnumerator _currentPrintingRoutine;

    public CommandState CurrentCommandState { get; set; } = CommandState.Unsent;

    protected override void Awake()
    {
        base.Awake();
        _commandText = GetComponentInChildren<TextMeshProUGUI>();
        _canvas = GetComponentInChildren<Canvas>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        base.Start();
        AssignSpriteStartSize();
    }

    public override void Select(bool fireEvent = true)
    {
        if (!_wasTeared)
        {
            _wasTeared = true;

            if (commandTeared != null)
                commandTeared();
        }

        base.Select(fireEvent);
    }

    private void LateUpdate()
    {
        if (!_wasTeared)
        {
            CheckForPrintingQueue();
        }
    }

    public void AssignCommandName(string commandName, string printText)
    {
        CommandName = commandName;
        PrintLine(printText);
    }

    public void AssignShipName(string shipName, string printText)
    {
        ShipName = shipName;
        PrintLine(printText);
    }

    public void AssignTargetGridCoords(Vector2 gridCoords, string printText)
    {
        TargetGridCoords = gridCoords.ToString();
        PrintLine(printText);
    }

    public void AssignProductCode(string productCode, string printText)
    {
        ProductCode = productCode;
        PrintLine(printText);
    }

    public void AssignRoute(List<string> route, string printText)
    {
        Route = route;
        PrintLine(printText);
    }

    public void AssignErrorMessage(string printText)
    {
        PrintLine(printText);
    }

    public void PrintLine(string newLine)
    {
        _printingQueue.Enqueue(newLine);
        _printQueueCount++;
    }

    private void CheckForPrintingQueue()
    {
        if (_printQueueCount > 0)
        {
            if (_currentPrintingRoutine != null)
                return;

            _printQueueCount--;
            _currentPrintingRoutine = PrintRoutine(_printingQueue.Dequeue());
            StartCoroutine(_currentPrintingRoutine);
        }
    }

    protected virtual void AssignSpriteStartSize()
    {
        _commandText.text = "a";
        _commandText.ForceMeshUpdate();
        Vector2 spriteSize = _spriteRenderer.size;
        spriteSize.y = _commandText.margin.y * 2;
        _spriteRenderer.size = spriteSize;
        _commandText.text = "";
        _commandText.ForceMeshUpdate();

        UpdateVisuals();
    }

    private void AssignSpriteSize()
    {
        _previousSpriteHeight = _spriteRenderer.size.y;
        Vector2 spriteSize = _spriteRenderer.size;
        spriteSize.y = _commandText.textBounds.size.y + _commandText.margin.y * 2;
        _spriteRenderer.size = spriteSize;

        UpdateVisuals();
    }

    private IEnumerator PrintRoutine(string newLine)
    {
        _collider.enabled = false;
        _commandText.text += newLine + "\n";
        _commandText.ForceMeshUpdate();
        //Debug.Log(_commandText.textBounds.extents);
        //Debug.Log(_commandText.margin);

        AssignSpriteSize();

        float heightDifference = _spriteRenderer.size.y - _previousSpriteHeight;
        float heightStep = heightDifference / 2f;

        Vector2 targetPos = transform.localPosition;
        targetPos.y += heightStep;

        if (commandSinglePrint != null)
            commandSinglePrint();

        while (Vector2.Distance(transform.localPosition, targetPos) > 0.001f)
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPos, 1f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.2f);

        targetPos = transform.localPosition;
        targetPos.y += heightStep;

        if (commandSinglePrint != null)
            commandSinglePrint();

        while (Vector2.Distance(transform.localPosition, targetPos) > 0.001f)
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPos, 1f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.2f);

        _collider.enabled = true;
        _currentPrintingRoutine = null;
    }

    public override void DisableObject()
    {
        base.DisableObject();
        _commandText.enabled = false;
        _canvas.enabled = false;
    }

    protected override void AssignToDropZone()
    {
        base.AssignToDropZone();
        if (_currentDropZone != null)
        {
            DropZone_Outbox drawer = _currentDropZone.GetComponent<DropZone_Outbox>();
            if (drawer != null)
                if (commandInOutbox != null)
                    commandInOutbox();
        }
    }

    protected virtual void UpdateVisuals()
    {
        Vector2 size = _spriteRenderer.size;
        size.x += 0.0625f;
        size.y += 0.0625f;
        _boxCollider2D.size = size;
        Vector2 offset = new Vector2();
        offset.x = _spriteRenderer.size.x / 2f;
        offset.y = _spriteRenderer.size.y / -2f;
        _boxCollider2D.offset = offset;

        _shadowRenderer.drawMode = SpriteDrawMode.Sliced;
        _shadowRenderer.size = _spriteRenderer.size;
    }
}

public enum CommandState
{
    Unsent,
    Sucess,
    Fail
}
