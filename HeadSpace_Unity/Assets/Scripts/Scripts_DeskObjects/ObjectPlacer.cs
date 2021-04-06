using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public static Action newObjectInOutbox;

    public static ObjectPlacer instance;
    private DropZone_Outbox _outbox;
    private DropZone_Drawer[] _drawers;

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
        PlayerInventory.placeObjectRequest += PlaceObject;
    }

    private void OnDisable()
    {
        MovableObject.placeObjectRequest -= PlaceObject;
        ShopManager.placeObjectRequest -= PlaceObject;
        PlayerInventory.placeObjectRequest -= PlaceObject;

        _outbox = FindObjectOfType<DropZone_Outbox>();
        _drawers = FindObjectsOfType<DropZone_Drawer>();
    }

    private void Start()
    {

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

            case ObjectSpawnZone.DrawerLeft:
                StartCoroutine(PlaceObjectInDrawer(obj, DrawerTray.left));
                break;

            case ObjectSpawnZone.DrawerCenter:
                StartCoroutine(PlaceObjectInDrawer(obj, DrawerTray.center));
                break;

            case ObjectSpawnZone.DrawerRight:
                StartCoroutine(PlaceObjectInDrawer(obj, DrawerTray.right));
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
            case ObjectSpawnZone.DrawerLeft:
            case ObjectSpawnZone.DrawerCenter:
            case ObjectSpawnZone.DrawerRight:
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

    private IEnumerator PlaceObjectInDrawer(MovableObject obj, DrawerTray tray)
    {
        DropZone_Drawer targetDrawer = null;

        if (_drawers == null)
        {
            _drawers = FindObjectsOfType<DropZone_Drawer>();

            if (_drawers == null)
                yield return null;
        }

        foreach (var drawer in _drawers)
        {
            if (drawer.tray == tray)
            {
                targetDrawer = drawer;
                break;
            }
        }

        //Debug.Log(obj.gameObject.name + " placed in drawer : " + targetDrawer.gameObject.name);

        if (targetDrawer != null)
        {
            obj.transform.position = targetDrawer.GetRandomPointInZone();
            obj.Select();

            yield return new WaitForFixedUpdate();
            obj.Deselect();
        }
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
    DrawerLeft,
    DrawerCenter,
    DrawerRight,
    Outbox,
    Desk,
    OutOfBounds
}
