using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static Action<GameObject, ObjectSpawnZone> placeObjectRequest;

    public static ShopManager instance;
    private BuyablesDatabase _buyablesDB;

    public bool TransactionInProgress { get; private set; }
    private int transactionInProgressCount;
    //public int spawnBoughtObjectsCooldown;

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        _buyablesDB = GetComponentInChildren<BuyablesDatabase>();
    }

    public bool TryBuyRequest(int objectCode)
    {
        if (_buyablesDB == null)
            return false;

        if (RessourceManager.instance == null)
            return false;

        BuyableObject objToSpawn;
        bool success = _buyablesDB.FindObjectWithCode(objectCode, out objToSpawn);

        if (success)
        {
            success = RessourceManager.instance.SpendCredits(objToSpawn.price);
            if (success)
            {
                TransactionInProgress = true;
                StartCoroutine(SpawnObject(objToSpawn));
            }
        }
        return success;
    }

    private IEnumerator SpawnObject(BuyableObject objToSpawn)
    {
        transactionInProgressCount++;
        yield return new WaitForSeconds(0f);

        for (int i = 0; i < objToSpawn.spawnQuantity; i++)
        {
            GameObject go = Instantiate(objToSpawn.objectPrefab);

            if (placeObjectRequest != null)
                placeObjectRequest(go, objToSpawn.spawnZone);
        }
        transactionInProgressCount--;

        yield return new WaitForSeconds(1f);
        
        if (transactionInProgressCount <= 0)
        {
            TransactionInProgress = false;
        }
    }
}
