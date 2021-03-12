using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableTool : InteractableObject
{
    [Header("Sliding settings")]
    public SlidingDirection slidingDirection;
    public float slidingAmount;
    public float drawerHandleSize = 1f;

    private float startPosX;
    private float startPosY;
    private float boxHalfSize;
    private bool isBeingHeld = false;
    private Vector3 mouseOffset;

    protected override void Start()
    {
        base.Start();
        
        startPosX = transform.position.x;
        startPosY = transform.position.y;

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
    }

    // Update is called once per frame
    void Update()
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
                        float clampedXPosLeft = Mathf.Clamp(mousePos.x + mouseOffset.x, horExtent - (slidingAmount/2f), horExtent + (slidingAmount / 2f) - drawerHandleSize);
                        gameObject.transform.position = new Vector3(clampedXPosLeft, transform.position.y, 0);
                        break;

                    case SlidingDirection.HorizontalRight:
                        float clampedXPosRight = Mathf.Clamp(mousePos.x + mouseOffset.x, -horExtent - (slidingAmount / 2f) + drawerHandleSize, -horExtent + (slidingAmount / 2f));
                        gameObject.transform.position = new Vector3(clampedXPosRight, transform.position.y, 0);
                        break;

                    case SlidingDirection.VerticalDown:
                        float clampedYPosDown = Mathf.Clamp(mousePos.y + mouseOffset.y, vertExtent - (slidingAmount / 2f), vertExtent + boxHalfSize - drawerHandleSize);
                        gameObject.transform.position = new Vector3(transform.position.x, clampedYPosDown, 0);
                        break;

                    case SlidingDirection.VerticalUp:
                        float clampedYPosUp = Mathf.Clamp(mousePos.y + mouseOffset.y, -vertExtent - (slidingAmount / 2f) + drawerHandleSize, -vertExtent + boxHalfSize);
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


    //private void OnMouseUp()
    //{
    //    isBeingHeld = false;
    //}

    //private void OnMouseDown()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 mousePos;

    //        mousePos = Input.mousePosition;
    //        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
    //        isBeingHeld = true;
    //    }

    //}
}
