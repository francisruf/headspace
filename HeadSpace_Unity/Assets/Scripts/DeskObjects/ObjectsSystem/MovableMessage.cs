using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMessage : MovableObject
{
    public override void Select() {
        base.Select();
        //Send action to receptor that he can print the next message
    }
}
