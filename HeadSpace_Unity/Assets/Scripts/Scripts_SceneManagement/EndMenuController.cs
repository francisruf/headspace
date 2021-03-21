using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndMenuController : MonoBehaviour
{
    public static Action playAgainButtonPressed;
    public static Action quitButtonPressed;


    public void OnPlayAgainButtonPressed()
    {
        if (playAgainButtonPressed != null)
            playAgainButtonPressed();
    }

    public void OnQuitButtonPressed()
    {
        if (quitButtonPressed != null)
            quitButtonPressed();
    }
}
