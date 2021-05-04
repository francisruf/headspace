using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Client
{
    public Action<Client> clientArrived;
    public Action<Client> clientStateChanged;
    public static Action clientEmbarked;

    public string clientFirstName;
    public string clientLastName;
    public Sprite clientSprite;
    public GridTile_Planet startPlanet;
    public GridTile_Planet endPlanet;
    public int maxCompletionTimeInGameMinutes;
    public ChallengeType challengeType;
    [HideInInspector] public ClientState currentState;

    public virtual string GetDisplayName()
    {
        return clientFirstName[0] + ". " + clientLastName;
    }

    public abstract string GetDestinationInfo();
    public abstract List<Sprite> GetDestinationCaracts();
    public abstract bool CheckSuccessCondition(GridTile_Planet currentPlanet);
    public abstract bool CheckStartPlanet(GridTile_Planet currentPlanet);

    public void ChangeState(ClientState newState)
    {
        if (currentState == ClientState.Debarked || currentState == ClientState.Dead)
            return;

        currentState = newState;
        switch (currentState)
        {
            case ClientState.Waiting:
                break;

            case ClientState.Embarked:
                if (clientEmbarked != null)
                    clientEmbarked();

                break;

            case ClientState.Debarked:
                break;
            case ClientState.Dead:
                break;

            default:
                break;
        }
        if (clientStateChanged != null)
            clientStateChanged(this);
    }
}

public enum ClientState
{
    Waiting,
    Embarked,
    Debarked,
    Dead
}

public enum ClientType
{
    Coords,
    PlanetName,
    PlanetCaract,
    Special
}
