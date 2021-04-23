using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSection : MonoBehaviour
{
    public List<Transform> allButtonSlots;
    public GameObject charPrefab;

    private int _buttonSlotsCount;
    private WritingMachineSpriteDB _spriteDB;

    private void Awake()
    {
        _buttonSlotsCount = allButtonSlots.Count;
        _spriteDB = GetComponentInParent<WritingMachineSpriteDB>();
    }

    public void AssignButtonPositions(List<ButtonController> buttons)
    {
        for (int i = 0; i < allButtonSlots.Count && i < buttons.Count; i++)
        {
            buttons[i].transform.parent = allButtonSlots[i];
            buttons[i].transform.localPosition = Vector3.zero;
        }
    }

    public void AssignCommandDisplaysPositions(List<ButtonController> buttons)
    {
        List<DisplayController_Command> displays = new List<DisplayController_Command>();
        foreach (var b in buttons)
        {
            DisplayController_Command disp = b.GetComponent<DisplayController_Command>();
            if (disp != null)
                displays.Add(disp);
        }

        if (displays.Count <= 0)
            return;

        Vector2 spawnPos = transform.position;
        int count = displays.Count;
        for (int i = 0; i < count; i++)
        {
            displays[i].InitializeDisplay(spawnPos);
            spawnPos.x += (_spriteDB.CommandLetterWidth + _spriteDB.CommandLetterSpace) * (displays[i].CharCount);

            if (i < count - 1)
            {
                SpriteRenderer emptySpace = Instantiate(charPrefab, spawnPos, Quaternion.identity, transform).GetComponent<SpriteRenderer>();
                emptySpace.sprite = _spriteDB.emptyCommandChar;
                spawnPos.x += _spriteDB.CommandLetterWidth + _spriteDB.CommandLetterSpace;
            }
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
