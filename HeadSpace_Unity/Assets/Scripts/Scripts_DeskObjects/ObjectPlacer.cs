using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public static Action newObjectInOutbox;

    public static ObjectPlacer instance;
    private DropZone_Outbox _outbox;
    private DropZone_Drawer _drawer;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        MovableObject.placeObjectRequest += PlaceObject;
        ShopManager.placeObjectRequest += PlaceObject;
    }

    private void OnDisable()
    {
        MovableObject.placeObjectRequest -= PlaceObject;
        ShopManager.placeObjectRequest -= PlaceObject;
    }

    private void Start()
    {
        _outbox = FindObjectOfType<DropZone_Outbox>();
        _drawer = FindObjectOfType<DropZone_Drawer>();
    }


    public void PlaceObject(MovableObject obj, ObjectSpawnZone targetZone)
    {
        switch (targetZone)
        {
            case ObjectSpawnZone.Desk:
                PlaceObjectInCenter(obj.gameObject);
                break;

            case ObjectSpawnZone.OutOfBounds:
                PlaceObjectOutOfBounds(obj.gameObject);
                break;

            case ObjectSpawnZone.Drawer:
                StartCoroutine(PlaceObjectInDrawer(obj));
                break;

            case ObjectSpawnZone.Outbox:
                StartCoroutine(PlaceObjectInOutbox(obj));
                break;

            default:
                break;
        }
    }

    public void PlaceObject(GameObject go, ObjectSpawnZone targetZone)
    {
        MovableObject candidate = go.GetComponentInChildren<MovableObject>();
        if (candidate != null)
        {
            PlaceObject(candidate, targetZone);
            return;
        }

        switch (targetZone)
        {
            case ObjectSpawnZone.Desk:
            case ObjectSpawnZone.Drawer:
            case ObjectSpawnZone.Outbox:
                PlaceObjectInCenter(go);
                break;

            case ObjectSpawnZone.OutOfBounds:
                PlaceObjectOutOfBounds(go);
                break;

            default:
                break;
        }
    }

    private IEnumerator PlaceObjectInDrawer(MovableObject obj)
    {
        if (_drawer == null)
        {
            _drawer = FindObjectOfType<DropZone_Drawer>();

            if (_drawer == null)
                yield return null;
        }
        obj.transform.position = _drawer.gameObject.transform.position;
        obj.Select();

        yield return new WaitForFixedUpdate();
        obj.Deselect();
    }

    private IEnumerator PlaceObjectInOutbox(MovableObject obj)
    {
        if (_outbox == null)
        {
            _outbox = FindObjectOfType<DropZone_Outbox>();

            if (_outbox == null)
                yield return null;
        }
        obj.transform.position = _outbox.gameObject.transform.position;
        obj.Select();

        yield return new WaitForFixedUpdate();
        obj.Deselect();

        if (newObjectInOutbox != null)
            newObjectInOutbox();
    }

    private void PlaceObjectInCenter(GameObject go)
    {
        go.transform.position = Vector2.zero;
    }

    private void PlaceObjectOutOfBounds(GameObject go)
    {
        go.transform.position = new Vector2(100f, 100f);
    }
}



public enum ObjectSpawnZone
{
    Drawer,
    Outbox,
    Desk,
    OutOfBounds
}
