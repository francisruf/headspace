using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logbook_ShipContainer : MonoBehaviour
{
    private Ship _linkedShip;
    public Ship LinkedShip { get { return _linkedShip; } }

    public Color deployedColor;
    public Color unloadingColor;
    public Color atBaseColor;
    public Color destroyedColor;

    public TextMeshProUGUI shipNameText;
    public TextMeshProUGUI shipCallsignText;
    public TextMeshProUGUI shipClassText;
    public TextMeshProUGUI shipStateText;
    public TMP_InputField shipNameField;
    public TMP_InputField shipCallsignField;

    public void InitializeContainer(Ship linkedShip)
    {
        _linkedShip = linkedShip;
        UpdateContainerUI();
    }

    public void ShipUpdate()
    {
        UpdateContainerUI();
    }

    private void UpdateContainerUI()
    {
        if (_linkedShip == null)
        {
            Debug.LogError("Could not find linked ship!");
            return;
        }

        shipNameField.text = _linkedShip.shipName;
        shipCallsignField.text = _linkedShip.shipCallsign;
        //shipNameText.text = _linkedShip.shipName;
        //shipCallsignText.text = _linkedShip.shipCallsign;
        shipClassText.text = "Explorer class";
        Color textColor;
        shipStateText.text = GetShipStatusFromState(_linkedShip.CurrentShipState, out textColor);
        shipStateText.color = textColor;
    }

    public void OnNameChange()
    {
        _linkedShip.ChangeShipName(shipNameField.text, shipCallsignField.text);
    }

    public void OnCallsignChange()
    {
        string newCallsign = shipCallsignField.text;
        int callsignLength = newCallsign.Length;

        while (callsignLength < 3)
        {
            newCallsign += "A";
            callsignLength++;
        }

        newCallsign = newCallsign.ToUpper();
        shipCallsignField.text = newCallsign;
        _linkedShip.ChangeShipName(shipNameField.text, newCallsign);
    }

    private string GetShipStatusFromState(ShipState targetState, out Color textColor)
    {
        string statusText = "";
        textColor = Color.black;

        switch (targetState)
        {
            case ShipState.Deployed:
                statusText = "Deployed";
                textColor = deployedColor;
                break;
            case ShipState.Unloading:
                statusText = "Unloading";
                textColor = unloadingColor;
                break;
            case ShipState.AtBase:
                statusText = "At Base";
                textColor = atBaseColor;
                break;
            case ShipState.Destroyed:
                statusText = "Destroyed";
                textColor = destroyedColor;
                break;
        }
        return statusText;
    }
}
