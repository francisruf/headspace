using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableMessage_Special : MovableMessage
{
    public static Action<string> specialMessageTeared;
    public string messageName;

    private void OnEnable()
    {
        TutorialController.messageShredEnable += OnMessageShredEnable;
    }

    private void OnDisable()
    {
        TutorialController.messageShredEnable -= OnMessageShredEnable;
    }

    protected override void TearMessage()
    {
        base.TearMessage();

        if (specialMessageTeared != null)
            specialMessageTeared(messageName);
    }

    private void OnMessageShredEnable(string messageName)
    {
        if (this.messageName == messageName)
            gameObject.tag = "Destruc";
    }
}
