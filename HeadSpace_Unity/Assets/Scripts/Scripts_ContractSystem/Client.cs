using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Client
{
    public string clientFirstName;
    public string clientLastName;
    public Sprite clientSprite;
    public GridTile_Planet startPlanet;
    public int maxCompletionTimeInGameMinutes;
    [HideInInspector] public ClientState currentState;

    public virtual string GetDisplayName()
    {
        return clientFirstName[0] + ". " + clientLastName;
    }

    public abstract string GetDestinationInfo();
    public abstract List<Sprite> GetDestinationCaracts();
    public abstract bool CheckSuccessCondition(GridTile_Planet currentPlanet);
}

public enum ClientState
{
    Waiting,
    PickedUp,
    Arrived,
    Dead
}

public enum ClientType
{
    Coords,
    PlanetName,
    PlanetCaract,
    Special
}
