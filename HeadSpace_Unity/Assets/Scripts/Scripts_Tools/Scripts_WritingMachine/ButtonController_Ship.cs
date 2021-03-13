using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController_Ship : ButtonController
{
    private Ship _linkedShip;

    public void InitializeShipButton(Ship ship)
    {
        _linkedShip = ship;
        BaseButtonText = _linkedShip.shipCallsign;
        _buttonTextMesh.text = BaseButtonText;
    }

    protected override void Start()
    {
        ChangeButtonState(ButtonState.Unavailable);
        nextButtonSection = ButtonSectionType.KeyPad;
    }
}
