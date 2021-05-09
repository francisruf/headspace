﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShredderSlot : MonoBehaviour
{
    public static Action shredderStarted;
    public static Action shredderStopped;

    public Transform minObjectPosY;
    public Transform maxObjectPosY;
    public Transform EndPosition;
    private Shredder parent = null;
    private Animator lightAnimator;
    private bool shredding = false;
    public bool canShred;

    private int _topSortOrder;

    private List<MovableObject> _shreddingObjects = new List<MovableObject>();
    private IEnumerator _currentStopDelay;

    private void Awake()
    {
        lightAnimator = GetComponentInParent<Animator>();
        if (parent == null)
        {
            parent = GetComponentInParent<Shredder>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14)
        {
            if (parent.InteractionsEnabled && collision.gameObject.tag == "Destruc")
            {
                if (canShred)
                {
                    MovableObject objToShred = collision.GetComponent<MovableObject>();
                    objToShred.Deselect();

                    Bounds objBounds = objToShred.ColliderBounds;
                    if (objBounds.size.y > objBounds.size.x)
                    {
                        objToShred.transform.Rotate(new Vector3(0f, 0f, 90f));
                    }
                    AssignObjectPosition(objToShred);

                    //objToShred.ObjSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
                    objToShred.SetSortingLayer(parent.ObjSpriteRenderer.sortingLayerID);
                    objToShred.SetOrderInLayer(_topSortOrder + 1);
                    _topSortOrder = objToShred.GetHighestOrder();
                    StartCoroutine(Shred(objToShred));
                }
            }
            else
            {
                lightAnimator.SetTrigger("Error");
            }
        }
    }

    private void AssignObjectPosition(MovableObject objToShred)
    {
        Bounds objBounds = objToShred.ObjSpriteRenderer.bounds;
        Vector2 centerOffset = objBounds.center - objToShred.transform.position;

        float minY = minObjectPosY.position.y + (objBounds.size.y / 2f) - centerOffset.y;
        float maxY = maxObjectPosY.position.y - (objBounds.size.y / 2f) - centerOffset.y;

        Vector2 newPos = objToShred.transform.position;
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        objToShred.transform.position = newPos;
        objToShred.transform.parent = this.transform.parent;
    }

    // TODO
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14)
        {
            UpdateLightState();
        }
    }

    private void ShredStart()
    {
        if (!shredding)
        {
            if (shredderStarted != null)
                shredderStarted();

            shredding = true;
        }
        UpdateLightState();
    }

    private void ShredEnd()
    {
        if (shredding)
        {
            bool stillShredding = _shreddingObjects.Count > 0;

            if (!stillShredding)
            {
                if (_currentStopDelay != null)
                    StopCoroutine(_currentStopDelay);

                _currentStopDelay = ShredStopDelay();
                StartCoroutine(_currentStopDelay);
            }
        }
        UpdateLightState();
    }

    private IEnumerator ShredStopDelay()
    {
        yield return new WaitForSeconds(0.02f);

        shredding = false;

        UpdateLightState();

        if (shredderStopped != null)
            shredderStopped();

        Debug.Log("STOP");
        _currentStopDelay = null;
    }

    private IEnumerator Shred(MovableObject obj)
    {
        _shreddingObjects.Add(obj);
        ShredStart();
        yield return StartCoroutine(LerpToPosition(obj));
        _shreddingObjects.Remove(obj);
        obj.DisableObject();
        ShredEnd();
    }

    private IEnumerator LerpToPosition(MovableObject obj)
    {
        Vector3 startingPosition = obj.transform.localPosition;
        Vector3 endingPosition = startingPosition;

        endingPosition.x -= obj.ObjSpriteRenderer.bounds.size.x + 0.1f;

        float longuestSide = obj.ObjSpriteRenderer.bounds.size.x;
        if (obj.ObjSpriteRenderer.bounds.size.y > longuestSide)
            longuestSide = obj.ObjSpriteRenderer.bounds.size.y;

        float timeElaps = 0f;
        float duration = 0.5f * longuestSide;
        while(timeElaps < duration)
        {
            obj.transform.localPosition = Vector3.Lerp(startingPosition, endingPosition, timeElaps / duration);
            timeElaps += Time.deltaTime;
            yield return null;
        }
    }

    public void TriggerLightsEnable()
    {
        lightAnimator.SetTrigger("Enable");
    }

    public void UpdateLightState()
    {
        lightAnimator.SetBool("InteractionsEnabled", parent.InteractionsEnabled);
        lightAnimator.SetBool("Shredding", shredding);
        lightAnimator.SetBool("CanShred", canShred);
    }

    //private bool CanShred()
    //{   //Determine if we can shred based on the position of the shredder compared to the total width of the camera
    //    float neededRatioValue = 0.15f;
    //    float ratio = 1 - (Mathf.Abs(parent.transform.position.x) / (Camera.main.orthographicSize * 2));
    //    return ratio > neededRatioValue;
    //}
}

public struct ShreddingRoutine
{
    public int instanceID;
    public IEnumerator routine;

    public ShreddingRoutine(int instanceID, IEnumerator routine)
    {
        this.instanceID = instanceID;
        this.routine = routine;
    }

    public void NullRoutine()
    {
        routine = null;
    }
}
