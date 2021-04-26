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
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= AddShip;
    }

    private void AddShip(Ship ship)
    {
        if (_currentSlots >= 3)
            return;

        allSlots[_currentSlots].AssignInfo(ship, _currentSlots);
        _currentSlots++;
    }
}
