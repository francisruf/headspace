using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidableOutbox : SlidableTool
{
    private DropZone_Outbox _dropZone;

    protected override void Awake()
    {
        base.Awake();
        _dropZone = GetComponentInChildren<DropZone_Outbox>();
    }

    protected override void FullyCloseTool()
    {
        base.FullyCloseTool();
        _dropZone.SendCommands();
    }
}
