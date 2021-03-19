using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ship Buyable", menuName = "BuyableObjects/ShipBuyable")]
public class BuyableShip : BuyableObject
{
    [Header("Ship properties")]
    [Range(1, 5)]
    public int scan;

    [Range(1, 5)]
    public int speed;

    [Range(1, 5)]
    public int cargo;
}
