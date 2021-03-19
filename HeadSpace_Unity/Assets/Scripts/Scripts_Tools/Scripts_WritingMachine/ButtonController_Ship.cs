using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController_Ship : ButtonController
{
    private Ship _linkedShip;

    private void OnEnable()
    {
        Ship.shipInfoChange += UpdateShipInfo;
    }

    private void OnDisable()
    {
        Ship.shipInfoChange -= UpdateShipInfo;
    }

    private void UpdateShipInfo(Ship ship)
    {
        if (ship != _linkedShip)
            return;

        BaseButtonText = _linkedShip.shipCallsign;
        _buttonTextMesh.text = _linkedShip.shipCallsign;
    }

    public void InitializeShipButton(Ship ship)
    {
        _linkedShip = ship;
        BaseButtonText = _linkedShip.shipCallsign;
        _buttonTextMesh.text = BaseButtonText;
    }

    public override string GetButtonCommandField()
    {
        return _linkedShip.shipName;
    }

    public override string GetButtonPrintText()
    {
        return printText + _linkedShip.shipName;
    }

    protected override void Start()
    {
        ChangeButtonState(ButtonState.Unavailable);
        nextButtonSection = ButtonSectionType.KeyPadVector;
    }
}
