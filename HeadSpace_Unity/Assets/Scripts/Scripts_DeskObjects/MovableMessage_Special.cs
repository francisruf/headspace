using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMessage_Special : MovableMessage
{
    public static Action<string> specialMessageTeared;
    public string messageName;

    protected override void TearMessage()
    {
        base.TearMessage();

        if (specialMessageTeared != null)
            specialMessageTeared(messageName);
    }
}
