using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableTool : InteractableObject
{
    [Header("SlidableTool settings")]
    public SlidingDirection slidingDirection;
    public float slidingAmount;
    public float drawerHandleSize = 1f;

    private float boxHalfSize;
    private Vector3 mouseOffset;
    private float openDistanceBuffer;

    protected float minPosX;
    protected float maxPosX;
    protected float minPosY;
    protected float maxPosY;
    protected Vector2 openPos = new Vector2();
    private Vector2 fullyClosedPos = new Vector2();

    public bool IsOpen { get; private set; }
    public bool IsFullyClosed { get; private set; }

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

        AssignStartingValues();
    }

    private void AssignStartingValues()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horExtent = vertExtent * Screen.width / Screen.height;

        openDistanceBuffer = 0.25f;

        switch (slidingDirection)
        {
            case SlidingDirection.HorizontalLeft:
                minPosX = horExtent - (slidingAmount / 2f);
                maxPosX = horExtent + (slidingAmount / 2f) - drawerHandleSize;
                break;

            case SlidingDirection.HorizontalRight:
                minPosX = -horExtent - (slidingAmount / 2f) + drawerHandleSize;
                maxPosX = -horExtent + (slidingAmount / 2f);
                break;

            case SlidingDirection.VerticalDown:
                minPosY = vertExtent - (slidingAmount / 2f);
                maxPosY = vertExtent + boxHalfSize - drawerHandleSize;
                break;

            case SlidingDirection.VerticalUp:
                minPosY = -vertExtent - (slidingAmount / 2f) + drawerHandleSize;
                maxPosY = -vertExtent + boxHalfSize;
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
                        maxPosX = horExtent + (slidingAmount / 2f) - drawerHandleSize;
                        float clampedXPosLeft = Mathf.Clamp(mousePos.x + mouseOffset.x, minPosX, maxPosX);
                        gameObject.transform.position = new Vector3(clampedXPosLeft, transform.position.y, 0);
                        break;

                    case SlidingDirection.HorizontalRight:
                        minPosX = -horExtent - (slidingAmount / 2f) + drawerHandleSize;
                        maxPosX = -horExtent + (slidingAmount / 2f);
                        float clampedXPosRight = Mathf.Clamp(mousePos.x + mouseOffset.x, minPosX, maxPosX);
                        gameObject.transform.position = new Vector3(clampedXPosRight, transform.position.y, 0);
                        break;

                    case SlidingDirection.VerticalDown:
                        minPosY = vertExtent - (slidingAmount / 2f);
                        maxPosY = vertExtent + boxHalfSize - drawerHandleSize;
                        float clampedYPosDown = Mathf.Clamp(mousePos.y + mouseOffset.y, minPosY, maxPosY);
                        gameObject.transform.position = new Vector3(transform.position.x, clampedYPosDown, 0);
                        break;

                    case SlidingDirection.VerticalUp:
                        minPosY = -vertExtent - (slidingAmount / 2f) + drawerHandleSize;
                        maxPosY = -vertExtent + boxHalfSize;
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
        bool fullyClosedDistance = false;

        switch (slidingDirection)
        {
            case SlidingDirection.VerticalDown:
                openPos.x = transform.position.x;
                openPos.y = minPosY;
                openDistance = Vector2.Distance(transform.position, openPos) <= openDistanceBuffer;

                fullyClosedPos.x = transform.position.x;
                fullyClosedPos.y = maxPosY;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= openDistanceBuffer;

                break;

            case SlidingDirection.VerticalUp:
                openPos.x = transform.position.x;
                openPos.y = maxPosY;
                openDistance = Vector2.Distance(transform.position, openPos) <= openDistanceBuffer;

                fullyClosedPos.x = transform.position.x;
                fullyClosedPos.y = minPosY;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= openDistanceBuffer;

                break;

            case SlidingDirection.HorizontalLeft:
                openPos.x = minPosX;
                openPos.y = transform.position.y;
                openDistance = Vector2.Distance(transform.position, openPos) <= openDistanceBuffer;

                fullyClosedPos.x = maxPosX;
                fullyClosedPos.y = transform.position.y;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= openDistanceBuffer;

                break;

            case SlidingDirection.HorizontalRight:
                openPos.x = maxPosX;
                openPos.y = transform.position.y;
                openDistance = Vector2.Distance(transform.position, openPos) <= openDistanceBuffer;

                fullyClosedPos.x = minPosX;
                fullyClosedPos.y = transform.position.y;
                fullyClosedDistance = Vector2.Distance(transform.position, fullyClosedPos) <= openDistanceBuffer;

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
    }

    public override void Select()
    {
        base.Select();
        mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public enum SlidingDirection
    {
        VerticalDown,  // 0 
        VerticalUp,   // 1
        HorizontalLeft,   // 2
        HorizontalRight   // 3
    }
}
