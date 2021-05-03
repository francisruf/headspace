using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New planet settings", menuName = "LevelSettings/Planet Settings")]
public class PlanetSettings : ScriptableObject
{
    [Header("Planet count")]
    public int planetTileCount;
    public int minTilesBetweenDeployPoint;
    [Tooltip("Used to calculate planet seperation. Use lower numbers on smaller maps.")]
    public int planetHeatDistance;

    [Header("Reveal settings")]
    public int planetsRevealedOnStart;
    public int planetsRevealedCloseToDeploy;
    public int planetsRevealedFarFromDeploy;
}
