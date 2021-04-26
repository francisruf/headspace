using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardReader : MonoBehaviour
{
    public static Action<bool> lightTick;

    public void TriggerSound(int isOn)
    {
        bool on = isOn == 1;

        if (lightTick != null)
            lightTick(on);
    }
}
