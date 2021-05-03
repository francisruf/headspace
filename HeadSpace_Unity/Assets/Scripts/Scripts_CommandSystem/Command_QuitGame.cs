using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_QuitGame : Command
{
    public static Action quitGameRequest;

    public override bool TryExecution(string playerText, out string errorMessage)
    {
        errorMessage = "";
        ExecuteCommand();
        return true;
    }

    public override bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage)
    {
        errorMessage = "";
        ExecuteCommand();
        return true;
    }

    protected override void ExecuteCommand()
    {
        if (quitGameRequest != null)
            quitGameRequest();
    }
}
