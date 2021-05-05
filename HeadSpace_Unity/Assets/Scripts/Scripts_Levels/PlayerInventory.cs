using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New inventory settings", menuName = "LevelSettings/Inventory Settings")]
public class PlayerInventory : ScriptableObject
{
    public static Action<GameObject, ObjectSpawnZone> placeObjectRequest; 

    [Header("Starting objects")]
    public List<ShipToSpawn> playerStartingShips = new List<ShipToSpawn>();
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
                Ship ship = go.GetComponent<Ship>();

                if (ship != null)
                    ship.spawnMarkerOnShipPosition = obj.spawnMarkerOnShipPosition;

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

[System.Serializable]
public struct ShipToSpawn
{
    public GameObject objectPrefab;
    public int quantity;
    public bool spawnMarkerOnShipPosition;
    public ObjectSpawnZone spawnZone;
}
