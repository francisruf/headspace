using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone_Drawer : DropZone
{
    public DrawerTray tray;
    public SpriteRenderer yellowNumber;
    public SpriteRenderer redNumber;

    public List<Sprite> yellowNumbers;
    public List<Sprite> redNumbers;

    public override void AddObjectToDropZone(MovableObject obj)
    {
        base.AddObjectToDropZone(obj);
        AssignObjectCount();
    }

    public override void RemoveObjectFromDropZone(MovableObject obj)
    {
        base.RemoveObjectFromDropZone(obj);
        AssignObjectCount();
    }

    private void AssignObjectCount()
    {
        if (tray == DrawerTray.top)
        {
            int index = Mathf.Clamp(_objectCount, 0, 10);
            yellowNumber.sprite = yellowNumbers[index];
        }
        else if (tray == DrawerTray.bottom)
        {
            int index = Mathf.Clamp(_objectCount, 0, 10);
            redNumber.sprite = redNumbers[index];
        }
    }
}

public enum DrawerTray
{
    top,
    bottom
}
