using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_EndTraining : Command
{
    public static Action endTrainingRequest;

    public override bool TryExecution(string playerText, out string errorMessage)
    {
        errorMessage = "";

        if (GameManager.instance != null)
            if (GameManager.instance.CurrentDayInfo.day == 0)
            {
                ExecuteCommand();
                return true;
            }
                

        return false;
    }

    public override bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage)
    {
        errorMessage = "";

        if (GameManager.instance != null)
            if (GameManager.instance.CurrentDayInfo.day == 0)
            {
                ExecuteCommand();
                return true;
            }

        return false;
    }

    protected override void ExecuteCommand()
    {
        if (endTrainingRequest != null)
            endTrainingRequest();
    }

}
