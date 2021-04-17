using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBoardSlot : MonoBehaviour
{
    private SlidableBoard _board;
    private ContractBoardSlot _currentContractSlots;
    public Ship LinkedShip { get; private set; }
    private List<MovableContract> allMovableContracts = new List<MovableContract>();

    private int _slotCount;
    private int _slotIndex;
    private int _contractCount;

    private void Awake()
    {
        _board = GetComponentInParent<SlidableBoard>();
    }

    public void AssignInfo(Ship linkedShip, int slotIndex)
    {
        LinkedShip = linkedShip;
        _slotIndex = slotIndex;
        _slotCount = LinkedShip.cargoCapacity;

        if (_currentContractSlots != null)
            _currentContractSlots.DisableSlots();

        SpawnContractSlots();
    }

    private void SpawnContractSlots()
    {
        if (_board == null)
            return;

        GameObject targetPrefab = null;

        switch (_slotCount)
        {
            case 0:
                break;
            case 1:
                targetPrefab = _board.singleContractSlotPrefab;
                break;
            case 2:
                targetPrefab = _board.doubleContractSlotPrefab;
                break;
            case 3:
                targetPrefab = _board.tripleContractSlotPrefab;
                break;
            default:
                break;
        }
        if (targetPrefab == null)
            return;

        _currentContractSlots = Instantiate(targetPrefab, transform).GetComponent<ContractBoardSlot>();
    }

    public void AssignContractToShip(MovableContract movableContract)
    {
        Contract contractInfo = movableContract.ContractInfo;
        LinkedShip.AssignContract(contractInfo);

        if (!allMovableContracts.Contains(movableContract))
        {
            allMovableContracts.Add(movableContract);
            _contractCount++;
        }

        AssignContractSlotsState();
    }

    public void RemoveContractFromShip(MovableContract movableContract)
    {
        Contract contractInfo = movableContract.ContractInfo;
        LinkedShip.RemoveContract(contractInfo);

        if (allMovableContracts.Remove(movableContract))
            _contractCount--;

        AssignContractSlotsState();
    }

    private void AssignContractSlotsState()
    {
        bool isActive = _contractCount <= 0 ? false : true;
        _currentContractSlots.AssignState(isActive);
    }
}
