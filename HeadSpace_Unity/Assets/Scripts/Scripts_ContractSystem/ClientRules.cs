using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientRules
{
    [Tooltip("Time required to spawn, calculated from previous contract spawn")]
    [Range(1, 120)]
    public int spawnTime;
    [Range(60, 600)]
    public int completionTimer;
    //public StartPlanetState startPlanetState;
    //public EndPlanetState endPlanetState;
    [Range(1, 3)]
    public int distanceRating;
    public ChallengeType challengeType;
    public SpecialConditions specialCondition;
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

public enum SpecialConditions
{
    None,
    ClosestPlanetToShip
}