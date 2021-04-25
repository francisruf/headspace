using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_CardReader : DropZone
{
    public static Action cardProcessed;

    public Transform cardLocation;
    private Animator _cardReaderAnimator;

    private bool _processed;
    private IEnumerator _currentCardRoutine;

    protected override void Awake()
    {
        base.Awake();
        _cardReaderAnimator = GetComponentInParent<Animator>();
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        obj.transform.position = cardLocation.position;
        obj.transform.parent = this.transform;
        obj.SetSortingLayer(SortingLayer.NameToID(targetSortingLayer));

        _cardReaderAnimator.SetBool("CardIn", true);

        if (!_processed)
        {
            _currentCardRoutine = ProcessCard(obj);
            StartCoroutine(_currentCardRoutine);
        }
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        obj.transform.parent = null;
        _cardReaderAnimator.SetBool("CardIn", false);

        if (_currentCardRoutine != null)
        {
            StopCoroutine(_currentCardRoutine);
            _currentCardRoutine = null;
        }
    }

    private IEnumerator ProcessCard(MovableObject obj)
    {
        yield return new WaitForSeconds(1.25f);
        _cardReaderAnimator.SetBool("Processed", true);
        _processed = true;
        obj.DisableInteractions();

        Debug.Log("Card process");

        if (cardProcessed != null)
            cardProcessed();

        _currentCardRoutine = null;
    }
}
