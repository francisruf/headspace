using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_CompletedContracts : DropZone
{
    private Animator _lightStickAnimator;
    private int contractIndex = 0;
    public static Action<int> newPointsInDropZone;

    protected override void Awake()
    {
        base.Awake();
        _lightStickAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        CreditsCounter.pointsCalculationEnd += OnPointsCalculationEnd;
    }

    private void OnDisable()
    {
        CreditsCounter.pointsCalculationEnd -= OnPointsCalculationEnd;
    }

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

        Contract contract = obj.GetComponent<MovableContract>().ContractInfo;

        if (contract.GetFinalPoints() >= 6)
        {
            _lightStickAnimator.SetBool("PointsCalculation", true);
            _lightStickAnimator.SetTrigger("NewContract");
        }
        else
        {
            _lightStickAnimator.SetTrigger("NewContractFail");
        }


        _objectCount++;
    }

    private void OnPointsCalculationEnd()
    {
        _lightStickAnimator.SetBool("PointsCalculation", false);
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
                            newPointsInDropZone(ctr.ContractInfo.GetFinalPoints());
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
