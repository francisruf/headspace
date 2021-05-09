using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfDemoController : MonoBehaviour
{
    public static Action quitPressed;
    public static Action<bool> mainMenuPressed;

    public void OnQuitButtonPressed()
    {
        if (quitPressed != null)
            quitPressed();
    }

    public void OnMainMenuButtonPressed()
    {
        //if (mainMenuPressed != null)
        //    mainMenuPressed(false);
    }
}
