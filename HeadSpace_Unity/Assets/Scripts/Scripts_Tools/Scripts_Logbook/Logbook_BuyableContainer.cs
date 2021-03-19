using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Logbook_BuyableContainer : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemCodeText;

    public void InitializeContainer(BuyableObject obj)
    {
        itemNameText.text = obj.buyableName;
        itemPriceText.text = "$ " + obj.price.ToString();
        itemDescriptionText.text = obj.description;
        itemCodeText.text = obj.code.ToString();
    }
}
