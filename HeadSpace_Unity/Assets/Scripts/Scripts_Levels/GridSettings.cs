using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New grid settings", menuName = "LevelSettings/Grid Settings")]
public class GridSettings : ScriptableObject
{
    [Header("Map size")]
    public int mapSizeX;
    public int mapSizeY;

    [Header("Base tilemaps")]
    public List<GameObject> tilemapPrefabs;

    [Header("Anomaly settings")]
    public int anomalySpreadTime;
}
