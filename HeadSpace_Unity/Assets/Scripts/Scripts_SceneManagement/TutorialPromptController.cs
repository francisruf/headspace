using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPromptController : MonoBehaviour
{
    public static Action<bool> tutorialPrompt;

    public void OnButtonPress(bool tutorial)
    {
        if (tutorialPrompt != null)
            tutorialPrompt(tutorial);
    }
}
