using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client_PlanetName : Client
{
    public override bool CheckSuccessCondition(GridTile_Planet currentPlanet)
    {
        if (currentPlanet == endPlanet)
            return true;

        return false;
    }

    public override List<Sprite> GetDestinationCaracts()
    {
        return null;
    }

    public override string GetDestinationInfo()
    {
        return endPlanet.PlanetName;
    }

    public override bool CheckStartPlanet(GridTile_Planet currentPlanet)
    {
        if (currentState != ClientState.Waiting)
            return false;

        if (currentPlanet == startPlanet)
            return true;

        return false;
    }
}
