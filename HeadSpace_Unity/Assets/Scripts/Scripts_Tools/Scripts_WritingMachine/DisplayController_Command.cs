using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayController_Command : ButtonController_Command
{
    public string baseDisplayText;
    public GameObject commandCharPrefab;

    public int CharCount { get { return baseDisplayText.Length; } }
    private int _currentLength;

    private WritingMachineSpriteDB _spriteDB;
    private List<CommandChar> _allChars = new List<CommandChar>();

    private void Awake()
    {
        BaseButtonText = baseDisplayText;
        _spriteDB = FindObjectOfType<WritingMachineSpriteDB>();
        //InitializeDisplay();
    }

    public void InitializeDisplay(Vector2 startPos)
    {
        Vector2 spawnPos = startPos;
        for (int i = 0; i < baseDisplayText.Length; i++)
        {
            CommandChar c = Instantiate(commandCharPrefab, spawnPos, Quaternion.identity, transform).GetComponent<CommandChar>();
            _allChars.Add(c);
            c.Initialize(_spriteDB.GetCommandChar(baseDisplayText[i]));
            spawnPos.x += _spriteDB.CommandLetterWidth + _spriteDB.CommandLetterSpace;
        }
    }

    public override void HighlightCharacter(int charIndex)
    {
        if (charIndex > _highlightedCharCount)
            return;

        //string newText = "<color=#EEDF84>";
        _currentLength = 0;

        for (int i = 0; i <= charIndex; i++)
        {
            _allChars[i].ToggleChar(true);
            _currentLength++;
        }

        //newText += "</color>";

        for (int i = _currentLength; i < BaseButtonText.Length; i++)
        {
            _allChars[i].ToggleChar(false);
            //newText += BaseButtonText[i];
        }

        _highlightedCharCount++;
        //_buttonTextMesh.text = newText;

        if (_highlightedCharCount >= BaseButtonText.Length)
        {
            ChangeButtonState(ButtonState.Ready);
        }
        else
        {
            ChangeButtonState(ButtonState.Selected);
        }
    }

    public override void ClearHighlighting()
    {
        for (int i = 0; i < _allChars.Count; i++)
        {
            _allChars[i].ToggleChar(false);
        }
        base.ClearHighlighting();
    }

    public override void ToggleAvailable(bool toggleON, bool forceDisable)
    {
        base.ToggleAvailable(toggleON, forceDisable);

        for (int i = 0; i < _allChars.Count; i++)
        {
            _allChars[i].ToggleChar(toggleON);
        }
    }

    public override void ChangeButtonState(ButtonState newState)
    {
        if (newState == CurrentState)
            return;

        CurrentState = newState;
    }
}
