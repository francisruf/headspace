using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client_Coords : Client
{
    public override bool CheckSuccessCondition(GridTile_Planet currentPlanet)
    {
        if (currentPlanet == endPlanet)
            return true;
        return false;
    }

    public override string GetDestinationInfo()
    {
        if (endPlanet != null)
            return endPlanet.tileName;
        else
            return "Somewhere";
    }

    public override List<Sprite> GetDestinationCaracts()
    {
        return null;
    }
}
