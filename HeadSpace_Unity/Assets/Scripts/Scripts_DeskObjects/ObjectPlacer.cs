using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    public static Action newObjectInOutbox;

    public static ObjectPlacer instance;
    private DropZone_Outbox _outbox;
    private SlidableDrawer _slidableDrawer;
    private DropZone_Drawer[] _drawers;
    private Vector2 _currentCenterSpawnPos = Vector2.zero;
    private Vector2 _currentDeskSpawnPos = new Vector2(6f, 0f);

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
        LevelManager.preLoadDone += AssignReferences;
        GridManager.newGameGrid += OnNewGrid;
    }

    private void OnDisable()
    {
        MovableObject.placeObjectRequest -= PlaceObject;
        ShopManager.placeObjectRequest -= PlaceObject;
        PlayerInventory.placeObjectRequest -= PlaceObject;
        LevelManager.preLoadDone -= AssignReferences;
        GridManager.newGameGrid -= OnNewGrid;
    }

    private void OnNewGrid(GridInfo info)
    {
        Vector3 offset = new Vector3(1f, -0.5f, 0f);
        _currentDeskSpawnPos = info.gameGridWorldBounds.max + offset;
    }

    private void Start()
    {

    }

    private void AssignReferences()
    {
        _outbox = FindObjectOfType<DropZone_Outbox>();
        _slidableDrawer = FindObjectOfType<SlidableDrawer>();
        _drawers = FindObjectsOfType<DropZone_Drawer>();
    }


    public void PlaceObject(MovableObject obj, ObjectSpawnZone targetZone)
    {
        switch (targetZone)
        {
            case ObjectSpawnZone.Desk:
                PlaceObjectOnDesk(obj);
                break;

            case ObjectSpawnZone.OutOfBounds:
                PlaceObjectOutOfBounds(obj.gameObject);
                break;

            case ObjectSpawnZone.DrawerTop:
                StartCoroutine(PlaceObjectInDrawer(obj, DrawerTray.top));
                break;

            case ObjectSpawnZone.DrawerBottom:
                StartCoroutine(PlaceObjectInDrawer(obj, DrawerTray.bottom));
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
                PlaceObjectOnDesk(go);
                break;
            case ObjectSpawnZone.DrawerTop:
            case ObjectSpawnZone.DrawerBottom:
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

        if (_slidableDrawer == null)
            _slidableDrawer = FindObjectOfType<SlidableDrawer>();

        if (_slidableDrawer.drawerType == DrawerType.Solo)
        {
            tray = DrawerTray.bottom;
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
            obj.Select(false);

            yield return new WaitForFixedUpdate();
            obj.Deselect(false);
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
        obj.Select(false);

        yield return new WaitForFixedUpdate();
        obj.Deselect(false);

        if (newObjectInOutbox != null)
            newObjectInOutbox();
    }

    private void PlaceObjectInCenter(GameObject go)
    {
        go.transform.position = _currentCenterSpawnPos;
    }

    private void PlaceObjectInCenter(MovableObject obj)
    {
        obj.transform.position = _currentCenterSpawnPos;
        obj.Select();
        obj.Deselect();
        _currentCenterSpawnPos.x += obj.ObjSpriteRenderer.bounds.size.x + 0.1f;
    }

    private void PlaceObjectOnDesk(GameObject go)
    {
        go.transform.position = _currentDeskSpawnPos;
    }

    private void PlaceObjectOnDesk(MovableObject obj)
    {
        obj.transform.position = _currentDeskSpawnPos;
        obj.Select();
        obj.Deselect();
        _currentDeskSpawnPos.y -= obj.ObjSpriteRenderer.bounds.size.y + 0.1f;
    }

    private void PlaceObjectOutOfBounds(GameObject go)
    {
        go.transform.position = new Vector2(100f, 100f);
    }
}



public enum ObjectSpawnZone
{
    DrawerTop,
    DrawerBottom,
    Outbox,
    Desk,
    OutOfBounds
}
