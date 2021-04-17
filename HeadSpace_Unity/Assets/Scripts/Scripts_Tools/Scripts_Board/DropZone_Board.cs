using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Board : DropZone
{
    public override void AddObjectToDropZone(MovableObject obj)
    {
        base.AddObjectToDropZone(obj);
        //Debug.Log("OBJECT ADDED TO BOARD");
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);
        //Debug.Log("OBJECT REMOVED FROM BOARD");
    }
}
