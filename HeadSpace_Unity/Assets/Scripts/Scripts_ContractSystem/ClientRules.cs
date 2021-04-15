using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientRules
{
    public StartPlanetState startPlanetState;
    public EndPlanetState endPlanetState;
    [Range(1, 3)]
    public int distanceRating;
    public ChallengeType challengeType;
}

public enum StartPlanetState
{
    Random,
    Found,
    NotFound
}

public enum EndPlanetState
{
    Random,
    Found,
    NotFound
}

public enum ChallengeType
{
    Random,
    Coords,
    PlanetName
}