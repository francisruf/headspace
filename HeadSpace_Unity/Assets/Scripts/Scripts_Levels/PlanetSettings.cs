using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New planet settings", menuName = "LevelSettings/Planet Settings")]
public class PlanetSettings : ScriptableObject
{
    public int planetsRevealedOnStart;
    public int planetsRevealedCloseToDeploy;
    public int planetsRevealedFarFromDeploy;
}
