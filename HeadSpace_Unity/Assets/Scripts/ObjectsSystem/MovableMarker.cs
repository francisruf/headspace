﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMarker : MovableObject
{
    protected override void Update()
    {
        base.Update();
    }

    public override void Select()
    {
        base.Select();
        _mouseOffset = new Vector2(0.04f, 0f);
        
        
    }

    public override void Deselect()
    {
        base.Deselect();
    }
}
