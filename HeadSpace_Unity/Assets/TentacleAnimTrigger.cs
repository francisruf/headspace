using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleAnimTrigger : MonoBehaviour
{
    public Action idleComplete;

    public void IdleComplete()
    {
        if (idleComplete != null)
            idleComplete();
    }
}
