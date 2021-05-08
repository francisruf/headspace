using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New contract settings", menuName = "LevelSettings/Contract Settings")]
public class ContractSettings : ScriptableObject
{
    public ContractSpawnCondition contractSpawnConditions;

    [Header("Time settings")]
    [Tooltip("When false, no time will be calculated.")]
    public bool timedContracts;

    [Tooltip("Time used when no client rule found.")]
    [Range(1, 120)]
    public int defaultContractSpawnInterval;
    public int completionTimeInSeconds;
    
    [Header("Client rules in spawn order")]
    public List<ClientRules> allClientRules;
}
