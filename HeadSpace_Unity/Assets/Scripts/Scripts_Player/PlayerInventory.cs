using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static Action<GameObject, ObjectSpawnZone> placeObjectRequest; 

    [Header("Starting objects")]
    public List<ObjectToSpawn> playerStartingShips = new List<ObjectToSpawn>();
    public List<ObjectToSpawn> playerDeskObjects = new List<ObjectToSpawn>();

    [Header("Starting ressources")]
    public int startingCredits;

    public void InitializeInventory()
    {
        GenerateStartingObjects();
        AssignStartingRessources();
    }

    private void GenerateStartingObjects()
    {
        foreach (var obj in playerStartingShips)
        {
            for (int i = 0; i < obj.quantity; i++)
            {
                GameObject go = Instantiate(obj.objectPrefab);

                if (placeObjectRequest != null)
                    placeObjectRequest(go, obj.spawnZone);
            }
        }

        foreach (var obj in playerDeskObjects)
        {
            for (int i = 0; i < obj.quantity; i++)
            {
                GameObject go = Instantiate(obj.objectPrefab);

                if (placeObjectRequest != null)
                    placeObjectRequest(go, obj.spawnZone);
            }
        }
    }

    private void AssignStartingRessources()
    {
        if (RessourceManager.instance != null)
            RessourceManager.instance.AddCredits(startingCredits, true);
    }
}

[System.Serializable]
public struct ObjectToSpawn
{
    public GameObject objectPrefab;
    public int quantity;
    public ObjectSpawnZone spawnZone;
}
