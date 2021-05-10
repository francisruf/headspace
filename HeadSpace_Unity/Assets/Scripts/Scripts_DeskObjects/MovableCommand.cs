using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovableCommand : MovableObject
{
    public Action<MovableCommand> commandTeared;
    public static Action<MovableCommand> commandTearTrigger;
    public static Action commandSinglePrint;
    public static Action commandInOutbox;

    [Header("MovableCommand settings")]
    public bool colliderStartEnabled;

    private TextMeshProUGUI _commandText;
    private Canvas _canvas;
    private BoxCollider2D _boxCollider2D;

    public float lineHeight;
    protected bool _wasTeared;
    private float _previousSpriteHeight;

    public Sprite baseSprite;
    public Sprite leftCornerSprite;
    public Sprite rightCornerSprite;

    [HideInInspector] public bool openDrawer = true;
    public string CommandName { get; protected set; } = "";
    public string ShipCallsign { get; private set; } = "";
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
        _collider.enabled = false;
    }

    protected override void Start()
    {
        base.Start();
        AssignSpriteStartSize();
        _collider.enabled = colliderStartEnabled;
    }

    public override void Select(bool fireEvent = true)
    {
        if (!_wasTeared)
        {
            _wasTeared = true;
            _spriteRenderer.sprite = baseSprite;

            if (commandTeared != null)
                commandTeared(this);

            if (commandTearTrigger != null)
                commandTearTrigger(this);
        }

        base.Select(fireEvent);
    }

    protected override void Update()
    {
        base.Update();

        if (!_wasTeared)
            CheckForMouse();
    }

    protected virtual void CheckForMouse()
    {
        if (CurrentCommandState == CommandState.HasLines)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (_boxCollider2D.bounds.Contains(mousePos))
            {
                float leftDistance = Mathf.Abs(mousePos.x - _boxCollider2D.bounds.min.x);
                float rightDistance = Mathf.Abs(mousePos.x - _boxCollider2D.bounds.max.x);

                if (leftDistance < rightDistance)
                {
                    _spriteRenderer.sprite = leftCornerSprite;
                }
                else
                {
                    _spriteRenderer.sprite = rightCornerSprite;
                }
            }
            else
            {
                _spriteRenderer.sprite = baseSprite;
            }
        }
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

    public void AssignShipCallsign(string shipCallsign, string printText)
    {
        ShipCallsign = shipCallsign;
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
        CurrentCommandState = CommandState.HasLines;
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

            if (_printQueueCount == 0)
                if (Vector2.Distance(transform.localPosition, targetPos) < heightStep / 1.35f && !_collider.enabled)
                    _collider.enabled = true;

            if (IsSelected)
                break;

            yield return new WaitForEndOfFrame();
        }

        if (!IsSelected)
            yield return new WaitForSeconds(0.2f);

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
    HasLines,
    Sucess,
    Fail
}
