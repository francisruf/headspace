using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableBoard : SlidableTool
{
    [Header("Prefabs")]
    public GameObject singleContractSlotPrefab;
    public GameObject doubleContractSlotPrefab;
    public GameObject tripleContractSlotPrefab;

    [Header("Slots")]
    public ShipBoardSlot[] allSlots = new ShipBoardSlot[4];
    private int _currentSlots;

    private void OnEnable()
    {
        Ship.newShipAvailable += AddShip;
        TutorialController.openBoardRequest += TriggerAutoOpen;
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= AddShip;
        TutorialController.openBoardRequest -= TriggerAutoOpen;
    }

    protected override IEnumerator AutoOpenTool()
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
        Vector2 halfOpenPos = openPos;
        halfOpenPos.y += 2f;

        while (Vector2.Distance(transform.position, halfOpenPos) > 0.01f)
        {
            Vector2 smooth = Vector2.SmoothDamp(transform.position, halfOpenPos, ref velocity, smoothTime);
            transform.position = smooth;

            yield return new WaitForEndOfFrame();
        }
        transform.position = halfOpenPos;
        _openingRoutine = null;
    }

    private void AddShip(Ship ship)
    {
        if (_currentSlots >= 3)
            return;

        allSlots[_currentSlots].AssignInfo(ship, _currentSlots);
        _currentSlots++;
    }
}
