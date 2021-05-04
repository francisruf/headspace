using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New inventory settings", menuName = "LevelSettings/Inventory Settings")]
public class PlayerInventory : ScriptableObject
{
    public static Action<GameObject, ObjectSpawnZone> placeObjectRequest; 

    [Header("Starting objects")]
    public List<ObjectToSpawn> playerStartingShips = new List<ObjectToSpawn>();
    public List<ObjectToSpawn> playerDeskObjects = new List<ObjectToSpawn>();


    public void InitializeInventory()
    {
        GenerateStartingObjects();
    }

    private void GenerateStartingObjects()
    {
        ObjectPlacer placer = ObjectPlacer.instance;

        foreach (var obj in playerStartingShips)
        {
            for (int i = 0; i < obj.quantity; i++)
            {
                GameObject go = Instantiate(obj.objectPrefab);

                if (placer != null)
                    placer.PlaceObject(go, obj.spawnZone);
            }
        }

        foreach (var obj in playerDeskObjects)
        {
            for (int i = 0; i < obj.quantity; i++)
            {
                GameObject go = Instantiate(obj.objectPrefab);

                if (placer != null)
                    placer.PlaceObject(go, obj.spawnZone);
            }
        }
    }
}

[System.Serializable]
public struct ObjectToSpawn
{
    public GameObject objectPrefab;
    public int quantity;
    public ObjectSpawnZone spawnZone;
}
