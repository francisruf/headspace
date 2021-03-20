using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public static Action playButtonPressed;
    public static Action quitButtonPressed;


    public void OnPlayButtonPressed()
    {
        if (playButtonPressed != null)
            playButtonPressed();
    }

    public void OnQuitButtonPressed()
    {
        if (quitButtonPressed != null)
            quitButtonPressed();
    }
}
