using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logbook_ShipContainer : MonoBehaviour
{
    private Ship _linkedShip;
    public Ship LinkedShip { get { return _linkedShip; } }

    public Color idleColor;
    public Color busyColor;
    public Color destroyedColor;

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
            case ShipState.Idle:
                statusText = "Idle";
                textColor = idleColor;
                break;
            case ShipState.Busy:
                statusText = "Busy";
                textColor = busyColor;
                break;
            case ShipState.Disabled:
                statusText = "Destroyed";
                textColor = destroyedColor;
                break;
        }
        return statusText;
    }
}
