using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Simple Buyable", menuName = "BuyableObjects/SimpleBuyable")]
public class BuyableObject : ScriptableObject
{
    [Header("General properties")]
    public string buyableName;
    public BuyableType buyableType;
    public string description;
    public Sprite icon;
    public int price;
    public int code;
    public int spawnQuantity;
    public ObjectSpawnZone spawnZone;

    [Header("Prefabs")]
    public GameObject objectPrefab;
    public GameObject objectFlyer;
}

public enum BuyableType
{
    Ship,
    DeskObject
}
