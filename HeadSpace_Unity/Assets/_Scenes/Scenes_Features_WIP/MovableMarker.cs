using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMarker : MovableObject
{
    // Start is called before the first frame update
    private void Start()
    {

    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Select()
    {
        base.Select();
        _mouseOffset = new Vector2(0.25f, 0);
        
        
    }

    public override void Deselect()
    {
        base.Deselect();
    }
}
