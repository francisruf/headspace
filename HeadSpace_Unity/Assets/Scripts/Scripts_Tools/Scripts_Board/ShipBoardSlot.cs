using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipBoardSlot : MonoBehaviour
{
    public Transform contractSlotPos;

    private SlidableBoard _board;
    private ContractBoardSlot _currentContractSlots;

    public Ship LinkedShip { get; private set; }
    private List<MovableContract> allMovableContracts = new List<MovableContract>();

    private Animator _shipDisplayAnimator;
    private TextMeshProUGUI _shipCallsignText;

    private int _slotCount;
    private int _slotIndex;
    private int _contractCount;
    private bool _destroyed;

    private void Awake()
    {
        _board = GetComponentInParent<SlidableBoard>();
        _shipDisplayAnimator = GetComponentInChildren<Animator>();
        _shipCallsignText = GetComponentInChildren<TextMeshProUGUI>();
        _shipDisplayAnimator.SetBool("ShipLinked", false);
        _shipCallsignText.text = "";
    }

    private void OnEnable()
    {
        Ship.shipStateChange += ChangeDisplayState;
        Ship.shipInfoChange += OnShipNameChange;
        Ship.shipDirectionChange += OnShipDirectionChange;
    }

    private void OnDisable()
    {
        Ship.shipStateChange -= ChangeDisplayState;
        Ship.shipInfoChange -= OnShipNameChange;
        Ship.shipDirectionChange -= OnShipDirectionChange;
    }

    public void AssignInfo(Ship linkedShip, int slotIndex)
    {
        LinkedShip = linkedShip;
        _slotIndex = slotIndex;
        _slotCount = LinkedShip.cargoCapacity;
        _shipCallsignText.text = LinkedShip.shipCallsign;
        _destroyed = linkedShip.CurrentShipState == ShipState.Disabled;

        if (_destroyed)
            _shipDisplayAnimator.SetBool("Destroyed", true);

        _shipDisplayAnimator.SetBool("ShipLinked", true);
        ChangeDisplayState(LinkedShip);

        if (_currentContractSlots != null)
            _currentContractSlots.DisableSlots();

        SpawnContractSlots();
    }

    private void OnShipNameChange(Ship ship)
    {
        if (ship != LinkedShip)
            return;

        if (LinkedShip == null)
            return;

        _shipCallsignText.text = LinkedShip.shipCallsign;
    }

    private void OnShipDirectionChange(Ship ship, MoveDirection dir)
    {
        if (ship != LinkedShip)
            return;

        if (LinkedShip == null)
            return;

        switch (dir)
        {
            case MoveDirection.Up:
                _shipDisplayAnimator.SetFloat("VerticalMove", 1.0f);
                _shipDisplayAnimator.SetFloat("HorizontalMove", 0.0f);
                break;
            case MoveDirection.Down:
                _shipDisplayAnimator.SetFloat("VerticalMove", -1.0f);
                _shipDisplayAnimator.SetFloat("HorizontalMove", 0.0f);
                break;
            case MoveDirection.Left:
                _shipDisplayAnimator.SetFloat("VerticalMove", 0.0f);
                _shipDisplayAnimator.SetFloat("HorizontalMove", -1.0f);
                break;
            case MoveDirection.Right:
                _shipDisplayAnimator.SetFloat("VerticalMove", 0.0f);
                _shipDisplayAnimator.SetFloat("HorizontalMove", 1.0f);
                break;
            case MoveDirection.None:
                _shipDisplayAnimator.SetFloat("VerticalMove", 0.0f);
                _shipDisplayAnimator.SetFloat("HorizontalMove", 0.0f);
                break;
        }
    }

    private void ChangeDisplayState(Ship ship)
    {
        if (ship != LinkedShip)
            return;

        switch (ship.CurrentShipState)
        {
            case ShipState.Idle:
                _shipDisplayAnimator.SetBool("Idle", true);
                _shipDisplayAnimator.SetBool("Busy", false);
                _shipDisplayAnimator.SetBool("Destroyed", false);
                _destroyed = false;
                break;
            case ShipState.Busy:
                _shipDisplayAnimator.SetBool("Idle", false);
                _shipDisplayAnimator.SetBool("Busy", true);
                _shipDisplayAnimator.SetBool("Destroyed", false);
                _destroyed = false;
                break;
            case ShipState.Disabled:
                _shipDisplayAnimator.SetBool("Destroyed", true);
                _destroyed = true;
                break;
            default:
                break;
        }
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

        _currentContractSlots = Instantiate(targetPrefab, contractSlotPos).GetComponent<ContractBoardSlot>();
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
