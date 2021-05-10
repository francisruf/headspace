using System;
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
    public bool shredding = false;
    public bool canShred;

    private int _topSortOrder;

    private List<MovableObject> _shreddingObjects = new List<MovableObject>();
    private IEnumerator _currentStopDelay;

    public GameObject particleSystemPrefab;
    private List<ParticleSystem> _allParticleSystems = new List<ParticleSystem>();

    private void Awake()
    {
        lightAnimator = GetComponentInParent<Animator>();
        if (parent == null)
        {
            parent = GetComponentInParent<Shredder>();
        }

        ParticleSystem ps = Instantiate(particleSystemPrefab, transform.parent).GetComponent<ParticleSystem>();
        _allParticleSystems.Add(ps);
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

        _currentStopDelay = null;
    }

    private IEnumerator Shred(MovableObject obj)
    {
        _shreddingObjects.Add(obj);
        ShredStart();

        Vector2 scale = obj.ColliderBounds.size;
        scale.x = 0.01f;
        Vector2 pos = obj.ColliderBounds.center;
        pos.x = obj.ColliderBounds.min.x;
        pos.y += 0.1f;
        pos.y = Mathf.Clamp(pos.y, minObjectPosY.position.y + (scale.y / 1.2f), maxObjectPosY.position.y) - (scale.y / 3f);
        Color psColor = obj.ObjSpriteRenderer.color;
        psColor.r -= 0.1f;
        psColor.g -= 0.1f;
        psColor.b -= 0.1f;


        ParticleSystem ps = GetParticleSystem();
        ps.transform.position = pos;
        var settings = ps.main;
        settings.startColor = psColor;
        var shape = ps.shape;
        shape.scale = scale;
        ps.Play();

        yield return StartCoroutine(LerpToPosition(obj));
        ps.Stop();

        _shreddingObjects.Remove(obj);
        obj.DisableObject();
        ShredEnd();
    }
    private IEnumerator LerpToPosition(MovableObject obj)
    {
        obj.DisableInteractions();

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
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(0.05f);
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

    private ParticleSystem GetParticleSystem()
    {
        foreach (var ps in _allParticleSystems)
        {
            if (!ps.isEmitting)
                return ps;
        }

        ParticleSystem newPs = Instantiate(particleSystemPrefab, transform.parent).GetComponent<ParticleSystem>();
        _allParticleSystems.Add(newPs);
        return newPs;
    }
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
