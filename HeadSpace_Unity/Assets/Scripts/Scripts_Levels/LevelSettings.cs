using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("Planets")]
    public PlanetSettings planetLevelSettings;
    [Header("Contracts")]
    public List<ClientRules> allClientRules;
}
