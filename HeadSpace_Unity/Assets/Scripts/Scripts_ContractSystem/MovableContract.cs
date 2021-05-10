using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableContract : MovableObject
{
    [Header("MovableContract settings")]
    public float animationSpeed;
    public LayerMask documentsLayerMask;
    public SpriteRenderer bottomLineRenderer;

    public static Action<MovableContract> contractOnBelt;
    public static Action<MovableContract> contractExitBelt;
    public static Action<MovableContract> contractAssigned;

    private IEnumerator _currentMovingRoutine;

    private Contract _contractInfo;
    public Contract ContractInfo { get { return _contractInfo; } }
    public int ClientAmount;
    private List<DropZone_ContractSlot> contractDropZones = new List<DropZone_ContractSlot>();
    private DropZone_Board boardDropZone;

    protected override void Awake()
    {
        base.Awake();
        _contractInfo = GetComponent<Contract>();
    }

    public virtual void InitializeContract(Vector2 animationEndPos)
    {
        _currentMovingRoutine = MoveContractOnBelt(animationEndPos);
        StartCoroutine(_currentMovingRoutine);
    }

    public override void Select(bool fireEvent = true)
    {
        if (_currentMovingRoutine != null)
        {
            StopCoroutine(_currentMovingRoutine);
            _currentMovingRoutine = null;

            SetSortingLayer(SortingLayer.NameToID("DeskObjects"));
            ObjectsManager.instance.ForceTopRenderingOrder(this);

            ContractExitBelt(false);
        }
        base.Select(fireEvent);

    }

    private IEnumerator MoveContractOnBelt(Vector2 animationEndPos)
    {
        if (contractOnBelt != null)
            contractOnBelt(this);

        //_spriteRenderer.sortingLayerID = SortingLayer.NameToID("ObjectsOnTools");

        while (Vector2.Distance(transform.position, animationEndPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, animationEndPos, animationSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = animationEndPos;

        Deselect();
        ContractExitBelt(true);

        _currentMovingRoutine = null;
    }

    private void ContractExitBelt(bool checkForContracts)
    {
        _contractInfo.OnContractBeltExit();

        if (contractExitBelt != null)
            contractExitBelt(this);

        if (!checkForContracts)
            return;
        bool placed = false;
        int count = 0;

        while (!placed)
        {
            placed = true;
            Collider2D[] colliders = Physics2D.OverlapPointAll(_collider.bounds.center, documentsLayerMask);
            foreach (var col in colliders)
            {
                MovableContract contract = col.GetComponent<MovableContract>();
                if (contract != null && contract != this)
                {
                    if (Vector2.Distance(contract.transform.position, transform.position) < 0.001f)
                    {
                        Vector2 newPos = contract.transform.position;
                        newPos.y += 0.03125f;
                        transform.position = newPos;
                        placed = false;
                        count++;
                        break;
                    }
                }
            }
            if (count > 400)
            {
                Debug.Log("OUCH");

                break;
            }
        }
    }

    protected override bool CheckForDropZone(out DropZone dropZone)
    {
        dropZone = null;
        bool found = false;

        Vector2 topCenterPoint = _spriteRenderer.bounds.center;
        topCenterPoint.y = _spriteRenderer.bounds.max.y + (-19 * (1 / 32f));// - ((ClientAmount - 1) * (38 * (1 / 32f)));

        //Debug.DrawLine(topCenterPoint, topCenterPoint + Vector2.up * 0.5f, Color.yellow, 5f);

        Collider2D[] allColliders = Physics2D.OverlapPointAll(topCenterPoint, dropZoneLayerMask);
        int colliderCount = allColliders.Length;

        for (int i = 0; i < colliderCount; i++)
        {
            DropZone candidate = allColliders[i].GetComponent<DropZone_ContractSlot>();

            if (candidate != null)
            {
                if (candidate.CheckIfAccepted(this))
                {
                    dropZone = candidate;
                    found = true;
                    break;
                }
            }
        }

        for (int i = 0; i < colliderCount; i++)
        {
            DropZone_Board candidate = allColliders[i].GetComponent<DropZone_Board>();
            if (candidate != null)
            {
                if (candidate.CheckIfAccepted(this))
                {
                    if (!found)
                    {
                        dropZone = candidate;
                        boardDropZone = candidate;
                        found = true;
                    }
                    else
                    {
                        boardDropZone = candidate;
                    }
                    break;
                }
            }
        }
        if (!found)
        {
            for (int i = 0; i < colliderCount; i++)
            {
                DropZone candidate = allColliders[i].GetComponent<DropZone>();
                if (candidate != null)
                {
                    if (candidate.CheckIfAccepted(this))
                    {
                        if (!found)
                        {
                            dropZone = candidate;
                            found = true;
                            break;
                        }
                    }
                }
            }
        }

        return found;
    }

    protected override void AssignToDropZone()
    {
        DropZone_ContractSlot contractDropZone = _currentDropZone.GetComponent<DropZone_ContractSlot>();

        if (contractDropZone != null)
        {
            if (contractDropZone.CheckIfContractAccepted(this, out contractDropZones))
            {
                foreach (var dz in contractDropZones)
                {
                    if (dz.ZoneIndex == 0)
                    {
                        bottomLineRenderer.enabled = false;
                    }
                    dz.AddObjectToDropZone(this);
                }

                Vector2 newPos = contractDropZones[0].ColliderBounds.max;
                newPos.x = contractDropZones[0].ColliderBounds.min.x;
                transform.position = newPos;
                boardDropZone = null;

                if (contractAssigned != null)
                    contractAssigned(this);

            }
            else if (boardDropZone != null)
            {
                boardDropZone.AddObjectToDropZone(this);
            }
        }

        else if (boardDropZone != null)
        {
            boardDropZone.AddObjectToDropZone(this);
        }

        else if (_currentDropZone != null)
            _currentDropZone.AddObjectToDropZone(this);
    }

    protected override void RemoveFromDropZone()
    {
        bottomLineRenderer.enabled = true;

        foreach (var dz in contractDropZones)
        {
            dz.Occupied = false;
            dz.RemoveObjectFromDropZone(this);
        }

        if (boardDropZone != null)
            boardDropZone.RemoveObjectFromDropZone(this);

        if (_currentDropZone != null)
            _currentDropZone.RemoveObjectFromDropZone(this);

        contractDropZones.Clear();
        _currentDropZone = null;
        boardDropZone = null;
    }
}
