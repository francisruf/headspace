using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logbook_BuyableShipContainer : Logbook_BuyableContainer
{
    public TextMeshProUGUI _scanValueText;
    public TextMeshProUGUI _speedValueText;
    public TextMeshProUGUI _cargoValueText;

    public void InitializeContainer(BuyableShip obj)
    {
        itemNameText.text = obj.buyableName;
        itemPriceText.text = "$ " + obj.price.ToString();
        itemDescriptionText.text = obj.description;
        itemCodeText.text = obj.code.ToString();

        int count = 0;
        string scanValue = "";
        while (count < obj.scan)
        {
            scanValue += "o ";
            count++;
        }
        _scanValueText.text = scanValue;

        count = 0;
        string speedValue = "";

        while (count < obj.speed)
        {
            speedValue += "o ";
            count++;
        }
        _speedValueText.text = speedValue;

        count = 0;
        string cargoValue = "";

        while (count < obj.cargo)
        {
            cargoValue += "o ";
            count++;
        }
        _cargoValueText.text = cargoValue;
    }
}
