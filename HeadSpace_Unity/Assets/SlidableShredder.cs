using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableShredder : SlidableTool
{
    private Vector3 mouseOffset;
    private float boxHalfSize;
    private ShredderSlot child = null;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        child = transform.GetComponentInChildren<ShredderSlot>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isSelected)
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
                        float clampedXPosLeft = Mathf.Clamp(mousePos.x + mouseOffset.x, horExtent - (slidingAmount / 2f), horExtent + (slidingAmount / 2f) - drawerHandleSize);
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
        if(child != null)
        {
            //Correct the sorting order in layer to always show the shredder slot
            child.GetComponent<SpriteRenderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        }
    }
}
