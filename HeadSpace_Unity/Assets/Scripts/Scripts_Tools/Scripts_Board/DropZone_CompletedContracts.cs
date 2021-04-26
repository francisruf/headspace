using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_CompletedContracts : DropZone
{
    private int contractIndex = 0;
    public static Action<int> newPointsInDropZone;

    public override void AddObjectToDropZone(MovableObject obj)
    {
        Vector2 pos = _collider.bounds.min;
        pos.y = _collider.bounds.max.y;
        pos.y += (1 / 32f) * contractIndex;
        obj.transform.position = pos;
        obj.transform.parent = this.transform;
        contractIndex++;

        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(HighestSortingOrder);
        HighestSortingOrder = obj.GetHighestOrder() + 1;

        obj.DisableInteractions();
    }

    public override bool CheckIfAccepted(MovableObject obj)
    {
        MovableContract ctr = obj.GetComponent<MovableContract>();
        if (ctr != null)
        {
            for (int i = 0; i < acceptedObjects.Length; i++)
            {
                if (acceptedObjects[i] == obj.objectType)
                {
                    if (ctr.ContractInfo.IsComplete)
                    {
                        if (newPointsInDropZone != null)
                            newPointsInDropZone(ctr.ContractInfo.PointsReward);
                        return true;
                    }
                        
                }
            }
        }
        return false;
    }
}
