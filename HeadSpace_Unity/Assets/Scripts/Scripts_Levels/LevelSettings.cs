using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [Header("General conditions")]
    public LevelEndCondition levelEndCondition;
    public ContractSpawnCondition contractSpawnConditions;
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

public enum ContractSpawnCondition
{
    Timed,
    Triggers
}
