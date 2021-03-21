using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShredderSlot : MonoBehaviour
{
    public static Action shredderStarted;

    public Transform minObjectPosY;
    public Transform maxObjectPosY;
    public Transform EndPosition;
    public SpriteRenderer GreenButton;
    public SpriteRenderer RedButton;
    private SlidableShredder parent = null;
    private bool shredding = false;
    public bool canShred;

    private LightState currentLightState = LightState.Red;

    private void Start()
    {
        GreenButton.enabled = false;
        RedButton.enabled = true;

        if (parent == null)
        {
            parent = transform.parent.GetComponent<SlidableShredder>();
        }
    }
    //private void Update()
    //{
    //    if (canShred || shredding)
    //    {
    //        GreenButton.enabled = false;
    //        RedButton.enabled = true;
    //    }
    //    else
    //    {
    //        GreenButton.enabled = true;
    //        RedButton.enabled = false;
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Destruc")

        {
            if(canShred)
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
                objToShred.SetOrderInLayer(parent.ObjSpriteRenderer.sortingOrder + 1);

                StartCoroutine(Shred(objToShred));
                //FindObjectOfType<AudioManager>().PlaySound("Shredder");
            }
        }

        // TODO
        if (collision.gameObject.layer == 14 && collision.gameObject.tag == "Indestruc")
        {
            ChangeLights(LightState.Red);
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
        shredding = true;
        UpdateLightState();
    }

    private void ShredEnd()
    {
        shredding = false;
        UpdateLightState();
    }

    private IEnumerator Shred(MovableObject obj)
    {
        ShredStart();

        if (shredderStarted != null)
            shredderStarted();

        yield return StartCoroutine(LerpToPosition(obj));
        ShredEnd();
        obj.DisableObject();
    }

    private IEnumerator LerpToPosition(MovableObject obj)
    {
        Vector3 startingPosition = obj.transform.localPosition;
        Vector3 endingPosition = startingPosition;

        endingPosition.x += obj.ObjSpriteRenderer.bounds.size.x + 0.1f;

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

    public void UpdateLightState()
    {
        if (canShred)
        {
            ChangeLights(LightState.Green);

            //if (shredding)
            //    ChangeLights(LightState.Red);

            //else
            //    ChangeLights(LightState.Green);
        }
        else
        {
            ChangeLights(LightState.Red);
        }
    }

    private void ChangeLights(LightState newState)
    {
        if (currentLightState == newState)
            return;

        currentLightState = newState;

        if (currentLightState == LightState.Red)
        {
            GreenButton.enabled = false;
            RedButton.enabled = true;
        }
        else
        {
            GreenButton.enabled = true;
            RedButton.enabled = false;
        }
    }

    private enum LightState
    {
        Red,
        Green
    }

    //private bool CanShred()
    //{   //Determine if we can shred based on the position of the shredder compared to the total width of the camera
    //    float neededRatioValue = 0.15f;
    //    float ratio = 1 - (Mathf.Abs(parent.transform.position.x) / (Camera.main.orthographicSize * 2));
    //    return ratio > neededRatioValue;
    //}
}
