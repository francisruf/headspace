using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_CardReader : DropZone
{
    public static Action cardProcessed;

    public Transform cardLocation;
    public float animSpeed;
    private Animator _cardReaderAnimator;
    private bool _cardInCollider;

    private bool _processed;
    private IEnumerator _currentCardRoutine;

    protected override void Awake()
    {
        base.Awake();
        _cardReaderAnimator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (_cardInCollider)
            return;

        MovableTimeCard card = collider.GetComponent<MovableTimeCard>();
        if (card != null)
        {
            card.Deselect(false);
            if (!_processed)
            {
                _currentCardRoutine = MoveAndProcessCard(card);
                StartCoroutine(_currentCardRoutine);
            }
            _cardInCollider = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        MovableTimeCard card = collider.GetComponent<MovableTimeCard>();
        if (card != null)
        {
            _cardInCollider = false;
        }
    }

    public override bool CheckIfAccepted(MovableObject obj)
    {
        // RETURN FALSE ALWAYS
        return false;
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        //obj.transform.position = cardLocation.position;
        obj.transform.parent = this.transform;
        obj.SetSortingLayer(SortingLayer.NameToID(targetSortingLayer));

        //_cardReaderAnimator.SetBool("CardIn", true);

        //if (!_processed)
        //{
        //    _currentCardRoutine = ProcessCard(obj);
        //    StartCoroutine(_currentCardRoutine);
        //}
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        obj.transform.parent = null;
        _cardReaderAnimator.SetBool("CardIn", false);
        //_collider.enabled = true;

        if (_currentCardRoutine != null)
        {
            StopCoroutine(_currentCardRoutine);
            _currentCardRoutine = null;
        }
    }

    private IEnumerator MoveAndProcessCard(MovableObject obj)
    {
        obj.ForceDropZone(this);
        AddObjectToDropZone(obj);

        Bounds objBounds = obj.ColliderBounds;
        Bounds colBounds = _collider.bounds;
        Vector2 centerOffset = objBounds.center - obj.transform.position;

        float minX = colBounds.min.x + (objBounds.size.x / 2f) - centerOffset.x;
        float maxX = colBounds.max.x - (objBounds.size.x / 2f) - centerOffset.x;

        //_collider.enabled = false;

        Vector2 newPos = obj.transform.position;
        //newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.x = colBounds.center.x;
        obj.transform.position = newPos;

        //Vector2 velocity = new Vector2();
        Vector2 finalPos = obj.transform.position;
        finalPos.y = cardLocation.position.y;

        while (Vector2.Distance(obj.transform.position, finalPos) > 0.001f)
        {
            if (obj.IsSelected)
            {
                Debug.Log("SELECTED");
                break;

            }
                

            obj.transform.position = Vector2.MoveTowards(obj.transform.position, finalPos, Time.deltaTime * animSpeed);
            yield return new WaitForEndOfFrame();
        }
        obj.transform.position = finalPos;

        _cardReaderAnimator.SetBool("CardIn", true);

        yield return new WaitForSeconds(1.5f);
        _cardReaderAnimator.SetBool("Processed", true);
        _processed = true;
        obj.DisableInteractions();



        if (cardProcessed != null)
            cardProcessed();

        _currentCardRoutine = null;
    }
}
