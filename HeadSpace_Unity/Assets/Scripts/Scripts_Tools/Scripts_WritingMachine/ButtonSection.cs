using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSection : MonoBehaviour
{
    public List<Transform> allButtonSlots;
    private int _buttonSlotsCount;

    private void Awake()
    {
        _buttonSlotsCount = allButtonSlots.Count;
    }

    public void AssignButtonPositions(List<ButtonController> buttons)
    {
        for (int i = 0; i < allButtonSlots.Count && i < buttons.Count; i++)
        {
            buttons[i].transform.parent = allButtonSlots[i];
            buttons[i].transform.localPosition = Vector3.zero;
        }
    }

    public void AssignToNextAvailableSlot(ButtonController button)
    {
        for (int i = 0; i < _buttonSlotsCount; i++)
        {
            if (allButtonSlots[i].transform.childCount == 0)
            {
                button.transform.parent = allButtonSlots[i];
                button.transform.localPosition = Vector3.zero;
                return;
            }
        }
    }
}

public enum ButtonSectionType
{
    Commands,
    Ships,
    KeyPadVector,
    KeyPadCode,
    RouteScreen,
    End
}
