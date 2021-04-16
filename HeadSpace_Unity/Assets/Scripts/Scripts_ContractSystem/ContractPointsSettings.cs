using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New points settings", menuName = "Contracts/Point Settings")]
public class ContractPointsSettings : ScriptableObject
{
    [Header("Points by CHALLENGE TYPE")]
    public int CoordsChallengePoints;
    public int PlanetNameChallengePoints;
    public int PlanetCaractChallengePoints;

    [Header("Points by START PLANET DISTANCE")]
    public int startShortDistancePoints;
    public int startMediumDistancePoints;
    public int startLongDistancePoints;

    [Header("Points by END PLANET DISTANCE")]
    public int endShortDistancePoints;
    public int endMediumDistancePoints;
    public int endLongDistancePoints;

    [Header("Points by TOTAL DISTANCE TRAVELLED")]
    public float pointsPerTile;
}
