using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableTool : InteractableObject
{
    public static Action<SlidableToolType> toolOpened;
    public static Action<SlidableToolType> toolClosed;
    public static Action<SlidableToolType> toolOpening;
    public static Action<SlidableToolType> toolClosing;

    public Action<SlidableTool> toolAutoOpened;

    [Header("SlidableTool settings")]
    public SlidingDirection slidingDirection;
    public float slidingAmount;
    public float drawerHandleSize = 1f;
    public SlidableToolType toolType;

    [Header("Animation settings")]
    public float smoothTime;

    [Header("Sound")]
    public string slidingSoundName = "";

    protected Vector2 _lerpStartPos;
    protected IEnumerator _openingRoutine;
    protected IEnumerator _closingRoutine;

    private float boxHalfSize;
    private Vector3 mouseOffset;
    public float openDistanceBuffer = 0.25f;

    protected float minPosX;
    protected float maxPosX;
    protected float minPosY;
    protected float maxPosY;
    protected Vector2 openPos = new Vector2();
    protected Vector2 fullyClosedPos = new Vector2();
    protected Vector2 fullyOpenPos = new Vector2();

    public bool IsOpen { get; private set; }
    public bool IsFullyClosed { get; private set; }
    public bool IsFullyOpen { get; private set; }

    private IEnumerator _movementRoutine;
    private Sound _moveSound;
    private bool _isPlayingLoop;

    protected override void Start()
    {
        base.Start();
       
        switch (slidingDirection)
        {
            case SlidingDirection.HorizontalLeft:
            case SlidingDirection.HorizontalRight:
                boxHalfSize = _spriteRenderer.bounds.size.x * 0.5f;
                break;
            case SlidingDirection.VerticalDown:
            case SlidingDirection.VerticalUp:
                boxHalfSize = _spriteRenderer.bounds.size.y * 0.5f;
                break;
            default:
                break;
        }
        fullyOpenPos = transform.position;
        fullyClosedPos = transform.position;
        AssignStartingValues();
        CheckOpenState();
        transform.position = fullyClosedPos;
    }

    private void AssignStartingValues()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horExtent = vertExtent * Screen.width / Screen.height;

        switch (slidingDirection)
        {
            case SlidingDirection.HorizontalLeft:
                minPosX = horExtent - (slidingAmount / 2f);
                maxPosX = fullyClosedPos.x;
                fullyOpenPos.x = minPosX;
                break;

            case SlidingDirection.HorizontalRight:
                minPosX = fullyClosedPos.x;
                maxPosX = -horExtent + (slidingAmount / 2f);
                fullyOpenPos.x = maxPosX;
                break;

            case SlidingDirection.VerticalDown:
                minPosY = vertExtent - (slidingAmount / 2f);
                maxPosY = fullyClosedPos.y;
                fullyOpenPos.y = minPosY;
                break;

            case SlidingDirection.VerticalUp:
                minPosY = fullyClosedPos.y;
                maxPosY = -vertExtent + (slidingAmount / 2f);
                fullyOpenPos.y = maxPosY;
                break;

            default:
                break;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(_isSelected)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 mousePos;

                mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);

                float vertExtent = Camera.main.orthographicSize;
                float horExtent = vertExtent * Screen.width / Screen.height;

                switch (slidingDirection)
                {
                    case SlidingDirection.HorizontalLeft:
                        minPosX = horExtent - (slidingAmount / 2f);
                        maxPosX = fullyClosedPos.x;
                        float clampedXPosLeft = Mathf.Clamp(mousePos.x + mouseOffset.x, minPosX, maxPosX);
                        gameObject.transform.position = new Vector3(clampedXPosLeft, transform.position.y, 0);
                        break;

                    case SlidingDirection.HorizontalRight:
                        minPosX = fullyClosedPos.x;
                        maxPosX = -horExtent + (slidingAmount / 2f);
                        float clampedXPosRight = Mathf.Clamp(mousePos.x + mouseOffset.x, minPosX, maxPosX);
                        gameObject.transform.position = new Vector3(clampedXPosRight, transform.position.y, 0);
                        break;

                    case SlidingDirection.VerticalDown:
                        minPosY = vertExtent - (slidingAmount / 2f);
                        maxPosY = fullyClosedPos.y;
                        float clampedYPosDown = Mathf.Clamp(mousePos.y + mouseOffset.y, minPosY, maxPosY);
                        gameObject.transform.position = new Vector3(transform.position.x, clampedYPosDown, 0);
                        break;

                    case SlidingDirection.VerticalUp:
                        minPosY = fullyClosedPos.y;
                        maxPosY = -vertExtent + (slidingAmount / 2f);
                        float clampedYPosUp = Mathf.Clamp(mousePos.y + mouseOffset.y, minPosY, maxPosY);
                        gameObject.transform.position = new Vector3(transform.position.x, clampedYPosUp, 0);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                Deselect();
            }
        }

        CheckOpenState();
    }

    protected void CheckOpenState()
    {
        bool openDistance = false;
        bool fullyOpenDistance = false;
        bool fullyClosedDistance = false;
        float distance = 0f;

        switch (slidingDirection)
        {
            case SlidingDirection.VerticalDown:
                openPos.x = transform.position.x;
                openPos.y = minPosY;
                distance = Vector2.Distance(transform.position, openPos);
                openDistance = distance <= openDistanceBuffer;
                fullyOpenDistance = distance <= 0.001f;

                //fullyClosedPos.x = transform.position.x;
                //fullyClosedPos.y = maxPosY;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= 0.001f;

                break;

            case SlidingDirection.VerticalUp:
                openPos.x = transform.position.x;
                openPos.y = maxPosY;
                distance = Vector2.Distance(transform.position, openPos);
                openDistance = distance <= openDistanceBuffer;
                fullyOpenDistance = distance <= 0.001f;

                //fullyClosedPos.x = transform.position.x;
                //fullyClosedPos.y = minPosY;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= 0.001f;

                break;

            case SlidingDirection.HorizontalLeft:
                openPos.x = minPosX;
                openPos.y = transform.position.y;
                distance = Vector2.Distance(transform.position, openPos);
                openDistance = distance <= openDistanceBuffer;
                fullyOpenDistance = distance <= 0.001f;

                //fullyClosedPos.x = maxPosX;
                //fullyClosedPos.y = transform.position.y;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= 0.001f;

                break;

            case SlidingDirection.HorizontalRight:
                openPos.x = maxPosX;
                openPos.y = transform.position.y;
                distance = Vector2.Distance(transform.position, openPos);
                openDistance = distance <= openDistanceBuffer;
                fullyOpenDistance = distance <= 0.001f;

                //fullyClosedPos.x = minPosX;
                //fullyClosedPos.y = transform.position.y;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= 0.001f;

                break;
        }

        if (openDistance && !IsOpen)
        {
            OpenTool();
        }
        else if (!openDistance && IsOpen)
        {
            CloseTool();
        }

        if (fullyClosedDistance && !IsFullyClosed)
        {
            FullyCloseTool();
        }
        else if (!fullyClosedDistance && IsFullyClosed)
        {
            IsFullyClosed = false;
        }

        if (fullyOpenDistance && !IsFullyOpen)
        {
            FullyOpenTool();
        }
        else if (!fullyOpenDistance && IsFullyOpen)
        {
            IsFullyOpen = false;
        }
    }

    protected virtual void OpenTool()
    {
        IsOpen = true;
        //Debug.Log(this.gameObject.name + " is OPEN.");

    }

    protected virtual void CloseTool()
    {
        IsOpen = false;
        //Debug.Log(this.gameObject.name + " is CLOSED.");
    }

    protected virtual void FullyCloseTool()
    {
        //Debug.Log(this.gameObject.name + "is FULLY CLOSED.");
        IsFullyClosed = true;

        if (toolClosed != null)
            toolClosed(toolType);
    }

    protected virtual void FullyOpenTool()
    {
        IsFullyOpen = true;

        if (toolOpened != null)
            toolOpened(toolType);
    }

    public override void Select(bool fireEvent = true)
    {
        base.Select(fireEvent);
        mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        _movementRoutine = CheckMovement();
        StartCoroutine(_movementRoutine);
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);
        if (_movementRoutine != null)
        {
            StopCoroutine(_movementRoutine);
            _movementRoutine = null;

            if (_isPlayingLoop)
            {
                AudioManager.instance.StopSoundLoop(_moveSound.name);
                _moveSound = null;
                _isPlayingLoop = false;
            }
        }
    }

    public enum SlidingDirection
    {
        VerticalDown,  // 0 
        VerticalUp,   // 1
        HorizontalLeft,   // 2
        HorizontalRight   // 3
    }

    public void TriggerAutoOpen()
    {
        if (_openingRoutine != null)
            return;

        if (_closingRoutine != null)
        {
            StopCoroutine(_closingRoutine);
            _closingRoutine = null;
        }

        _openingRoutine = AutoOpenTool();
        StartCoroutine(_openingRoutine);
    }

    public void TriggerAutoClose()
    {
        if (_closingRoutine != null)
            return;

        if (_openingRoutine != null)
        {
            StopCoroutine(_openingRoutine);
            _openingRoutine = null;
        }

        _closingRoutine = AutoCloseTool();
        StartCoroutine(_closingRoutine);
    }

    protected virtual IEnumerator AutoOpenTool()
    {
        if (!IsOpen)
        {
            if (toolAutoOpened != null)
                toolAutoOpened(this);

            if (toolOpening != null)
                toolOpening(toolType);
        }

        CheckOpenState();
        _lerpStartPos = transform.position;
        //float time = 0f;
        Vector2 velocity = new Vector2();

        while (Vector2.Distance(transform.position, openPos) > 0.01f)
        {
            Vector2 smooth = Vector2.SmoothDamp(transform.position, openPos, ref velocity, smoothTime);
            transform.position = smooth;

            yield return new WaitForEndOfFrame();
        }
        transform.position = openPos;
        _openingRoutine = null;
    }

    protected virtual IEnumerator AutoCloseTool()
    {
        if (IsOpen)
        {
            if (toolAutoOpened != null)
                toolAutoOpened(this);

            if (toolClosing != null)
                toolClosing(toolType);
        }

        CheckOpenState();
        _lerpStartPos = transform.position;
        //float time = 0f;
        Vector2 velocity = new Vector2();

        while (Vector2.Distance(transform.position, fullyClosedPos) > 0.01f)
        {
            Vector2 smooth = Vector2.SmoothDamp(transform.position, fullyClosedPos, ref velocity, smoothTime);
            transform.position = smooth;

            yield return new WaitForEndOfFrame();
        }
        transform.position = fullyClosedPos;
        _closingRoutine = null;
    }

    protected IEnumerator CheckMovement()
    {
        bool moving = false;
        bool openEventFired = false;
        bool closeEventFired = false;
        float distance;
        float time = 0f;

        while (true)
        {
            if (openEventFired && closeEventFired)
            {
                Vector2 startPos2 = transform.position;
                yield return new WaitForSeconds(0.0125f);
                Vector2 currentPos2 = transform.position;
                distance = Vector2.Distance(startPos2, currentPos2);

                if (distance < 0.025f)
                    time += 0.0075f;
                else
                    time = 0f;

                if (time >= 0.15f)
                {
                    openEventFired = false;
                    closeEventFired = false;
                }
                continue;
            }

            Vector2 startPos = transform.position;
            yield return new WaitForSeconds(0.025f);
            Vector2 currentPos = transform.position;
            distance = Vector2.Distance(startPos, currentPos);
            moving = distance > 0.01250f;
            bool opening = false;

            switch (slidingDirection)
            {
                case SlidingDirection.VerticalDown:
                    opening = currentPos.y < startPos.y;
                    break;
                case SlidingDirection.VerticalUp:
                    opening = currentPos.y > startPos.y;
                    break;
                case SlidingDirection.HorizontalLeft:
                    opening = currentPos.x < startPos.x;
                    break;
                case SlidingDirection.HorizontalRight:
                    opening = currentPos.x > startPos.x;
                    break;
            }

            if (moving)
            {
                if (!openEventFired && opening)
                {
                    openEventFired = true;
                    if (toolOpening != null)
                        toolOpening(toolType);

                    yield return new WaitForSeconds(0.5f);
                }
                else if (!closeEventFired && !opening)
                {
                    closeEventFired = true;
                    if (toolClosing != null)
                        toolClosing(toolType);

                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}

public enum SlidableToolType
{
    Drawer,
    Outbox,
    WritingMachine,
    Board,
    Shredder
}
