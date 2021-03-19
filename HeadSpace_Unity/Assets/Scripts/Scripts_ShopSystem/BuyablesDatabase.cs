using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuyablesDatabase : MonoBehaviour
{
    public static BuyablesDatabase instance;

    public static Action<Dictionary<int, BuyableShip>> newShipyardDB;
    public static Action<Dictionary<int, BuyableObject>> newCatalogueDB;

    public List<BuyableObject> allBuyables;
    public List<BuyableShip> allBuyableShips;

    private Dictionary<int, BuyableObject> _catalogueDatabase = new Dictionary<int, BuyableObject>();
    public Dictionary<int, BuyableObject> CatalogueDatabase { get { return _catalogueDatabase; } }
    private Dictionary<int, BuyableShip> _shipyardDatabase = new Dictionary<int, BuyableShip>();
    public Dictionary<int, BuyableShip> ShipyardDatabase { get { return _shipyardDatabase; } }

    private void OnEnable()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        foreach (var item in allBuyables)
        {
            _catalogueDatabase.Add(item.code, item);
        }

        foreach (var item in allBuyableShips)
        {
            _shipyardDatabase.Add(item.code, item);
        }

        //if (newShipyardDB != null)
        //    newShipyardDB(_shipyardDatabase);

        //if (newCatalogueDB != null)
        //    newCatalogueDB(_catalogueDatabase);
    }

    public bool FindObjectWithCode(int code, out BuyableObject obj)
    {
        obj = null;

        if (_catalogueDatabase.ContainsKey(code))
            obj = _catalogueDatabase[code];

        else if (_shipyardDatabase.ContainsKey(code))
            obj = _shipyardDatabase[code];

        if (obj != null)
            return true;

        return false;
    }
}
