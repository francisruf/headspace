using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_ContractSlot : DropZone
{
    private ShipBoardSlot linkedShipBoardSlot;
    public DropZone_ContractSlot[] NeighbourZones { get; set; }
    public int ZoneIndex { get; set; }
    public bool Occupied { get; set; }
    public int NeighbourCount { get; set; }

    protected override void Awake()
    {
        base.Awake();
        linkedShipBoardSlot = transform.parent.GetComponentInParent<ShipBoardSlot>();
    }

    public bool CheckIfContractAccepted(MovableContract contract, out List<DropZone_ContractSlot> contractDropZones)
    {
        contractDropZones = new List<DropZone_ContractSlot>();
        int size = contract.ClientAmount;
        int count = 0;

        for (int i = ZoneIndex; i >= ZoneIndex - size + 1; i--)
        {
            if (NeighbourZones[i].Occupied)
                return false;

            count++;

            if (i == 0 && count < size)
                return false;
        }

        if (count >= contract.ClientAmount)
        {
            for (int i = ZoneIndex; i >= ZoneIndex - count + 1; i--)
            {
                NeighbourZones[i].Occupied = true;
                contractDropZones.Add(NeighbourZones[i]);
            }
            linkedShipBoardSlot.AssignContractToShip(contract);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        obj.transform.parent = this.transform;

        //Debug.Log("Container : " + _containerSpriteRenderer.gameObject.name);
        int sortingOrder = _containerSpriteRenderer.sortingOrder + 1;
        sortingOrder += obj.RendererAmount * ((NeighbourCount - 1) - ZoneIndex);

        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(sortingOrder);
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);

        MovableContract ctr = obj.GetComponent<MovableContract>();
        if (ctr != null)
        {
            linkedShipBoardSlot.RemoveContractFromShip(ctr);
        }
    }
}
