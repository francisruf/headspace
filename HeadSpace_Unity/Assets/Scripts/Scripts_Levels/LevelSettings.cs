using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("EndCondition")]
    public LevelEndCondition levelEndCondition;
    [Header("Grid")]
    public GridSettings gridSettings;
    [Header("Planets")]
    public PlanetSettings planetLevelSettings;
    [Header("Inventory")]
    public PlayerInventory inventorySettings;
    [Header("Contracts")]
    public List<ClientRules> allClientRules;
}

public enum LevelEndCondition
{
    Time,
    Event
}
