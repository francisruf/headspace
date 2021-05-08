using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Board_Exclusion : DropZone
{
    //public DropZone_Board rejectedDropZone;
    public Collider2D topCollider;
    public Collider2D centerCollider;
    public Collider2D bottomCollider;

    protected override void Awake()
    {
        base.Awake();
        _activeCollider = bottomCollider;
    }

    public override void AddObjectToDropZone(MovableObject obj)
    {
        if (obj.ObjSpriteRenderer.bounds.size.y <= bottomCollider.bounds.size.y)
        {
            if (obj.ObjSpriteRenderer.bounds.center.y > centerCollider.bounds.center.y)
                _activeCollider = topCollider;
            else
            _activeCollider = bottomCollider;
        }
        else
            _activeCollider = topCollider;

        base.AddObjectToDropZone(obj);
        //Debug.Log("OBJECT ADDED TO BOARD");
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);
        //Debug.Log("OBJECT REMOVED FROM BOARD");
    }
}
