using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineSpriteDB : MonoBehaviour
{
    [Header("Main body")]
    //public Sprite emptyCommandLetter;
    public Sprite emptyCommandChar;
    public Sprite commandCharCaretOff;
    public Sprite commandCharCaretOn;
    [SerializeField] private List<CharMatch> _commandLetters;

    [Header("Route screen")]
    public Sprite emptyCoord;
    [SerializeField] private List<Sprite> _coordLetters;
    [SerializeField] private List<Sprite> _coordNumbers;

    [Header("Clock")]
    public Sprite defaultRegularDigit;
    public Sprite defaultInvertedDigit;
    [SerializeField] private List<Sprite> _regularDigits;
    [SerializeField] private List<Sprite> _invertedDigits;

    public const float COMMAND_LETTER_WIDTH = 0.1875f;
    public const float COMMAND_LETTER_SPACE = 0.03125f;
    public float CommandLetterWidth { get { return COMMAND_LETTER_WIDTH; } }
    public float CommandLetterSpace { get { return COMMAND_LETTER_SPACE; } }

    public CharMatch GetCommandChar(char c)
    {
        CharMatch targetMatch = default;
        c = char.ToLower(c);

        foreach (var letter in _commandLetters)
        {
            if (letter.charName == c)
            {
                targetMatch = letter;
                break;
            }
        }
        if (targetMatch.Equals(default(CharMatch)))
            targetMatch = new CharMatch('!', emptyCommandChar, emptyCommandChar);

        return targetMatch;
    }

    public Sprite GetCoordChar(char c)
    {
        c = char.ToUpper(c);
        Sprite targetSprite = null;

        if (c >= 'A' && c <= 'Z')
        {
            int index = Mathf.Abs((int)'A' - (int)c);
            targetSprite = _coordLetters[index];
        }

        else if (c >= '0' && c <= '9')
        {
            int index = (int)c - 48;
            targetSprite = _coordNumbers[index];
        }

        if (targetSprite == null)
            targetSprite = emptyCoord;

        return targetSprite;
    }

    public Sprite GetDigit(int digit, bool regularDigit)
    {
        Sprite targetSprite = null;

        if (regularDigit)
        {
            targetSprite = defaultRegularDigit;
            if (digit >= 0 && digit <= 9)
            {
                targetSprite = _regularDigits[digit];
            }
        }
        else
        {
            targetSprite = defaultInvertedDigit;
            if (digit >= 0 && digit <= 9)
            {
                targetSprite = _invertedDigits[digit];
            }
        }

        return targetSprite;
    }
}

[System.Serializable]
public struct CharMatch
{
    public char charName;
    public Sprite offSprite;
    public Sprite onSprite;

    public CharMatch(char charName, Sprite offSprite, Sprite onSprite)
    {
        this.charName = charName;
        this.offSprite = offSprite;
        this.onSprite = onSprite;
    }
}
