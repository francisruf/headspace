using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableContract : MovableObject
{
    [Header("MovableContract settings")]
    public float animationSpeed;
    public LayerMask documentsLayerMask;

    public static Action<MovableContract> contractOnBelt;
    public static Action<MovableContract> contractExitBelt;

    private IEnumerator _currentMovingRoutine;

    public virtual void InitializeContract(Vector2 animationEndPos)
    {
        _currentMovingRoutine = MoveContractOnBelt(animationEndPos);
        StartCoroutine(_currentMovingRoutine);
    }

    public override void Select()
    {
        if (_currentMovingRoutine != null)
        {
            StopCoroutine(_currentMovingRoutine);
            _currentMovingRoutine = null;

            SetSortingLayer(SortingLayer.NameToID("DeskObjects"));
            ObjectsManager.instance.ForceTopRenderingOrder(this);

            if (contractExitBelt != null)
                contractExitBelt(this);
        }
        base.Select();
    }

    private IEnumerator MoveContractOnBelt(Vector2 animationEndPos)
    {
        if (contractOnBelt != null)
            contractOnBelt(this);

        SetSortingLayer(SortingLayer.NameToID("ObjectsOnTools"));
        ObjectsManager.instance.ForceTopRenderingOrder(this);

        //_spriteRenderer.sortingLayerID = SortingLayer.NameToID("ObjectsOnTools");

        while (Vector2.Distance(transform.position, animationEndPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, animationEndPos, animationSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = animationEndPos;

        SetSortingLayer(SortingLayer.NameToID("DeskObjects"));
        ObjectsManager.instance.ForceTopRenderingOrder(this);

        //_spriteRenderer.sortingLayerID = SortingLayer.NameToID("DeskObjects");

        //List<Collider2D> allOverlappedColliders = new List<Collider2D>();
        //ContactFilter2D filter = new ContactFilter2D();
        //filter.SetLayerMask(documentsLayerMask);
        //int colliderCount = _collider.OverlapCollider(filter, allOverlappedColliders);

        //MovableContract latestContract = null;
        //foreach (var col in allOverlappedColliders)
        //{
        //    MovableContract other = col.GetComponent<MovableContract>();
        //    if (other != null)
        //    {
        //        if (latestContract == null)
        //            latestContract = other;
        //        else
        //        {
        //            if (other.ContractID > latestContract.ContractID)
        //                latestContract = other;
        //        }
        //    }
        //}

        //if (latestContract != null)
        //{
        //    Vector2 offset = new Vector2(0f, 1 / 32f);
        //    transform.position = latestContract.transform.position + (Vector3)offset;
        //}

        if (contractExitBelt != null)
            contractExitBelt(this);

        _currentMovingRoutine = null;
    }
}
