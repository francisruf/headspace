using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonController : MonoBehaviour
{
    protected TextMeshProUGUI _buttonTextMesh;
    protected Animator _animator;
    public string BaseButtonText { get; protected set; }
    protected int _highlightedCharCount;
    public ButtonState CurrentState { get; protected set; }

    public ButtonSectionType nextButtonSection;

    private void Awake()
    {
        _buttonTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        _animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        BaseButtonText = _buttonTextMesh.text;
        ChangeButtonState(ButtonState.Unavailable);
    }

    public void HighlightCharacter(int charIndex)
    {
        if (charIndex > _highlightedCharCount)
            return;

        string newText = "<color=#EEDF84>";
        int charCount = 0;

        for (int i = 0; i <= charIndex; i++)
        {
            newText += BaseButtonText[i];
            charCount++;
        }

        newText += "</color>";

        for (int i = charCount; i < BaseButtonText.Length; i++)
        {
            newText += BaseButtonText[i];
        }

        _highlightedCharCount++;
        _buttonTextMesh.text = newText;

        if (_highlightedCharCount >= BaseButtonText.Length)
        {
            _animator.SetBool("IsReady", true);
            ChangeButtonState(ButtonState.Ready);
        }
        else
        {
            ChangeButtonState(ButtonState.Selected);
        }
    }

    public void ClearHighlighting()
    {
        _buttonTextMesh.text = BaseButtonText;
        _highlightedCharCount = 0;

        ChangeButtonState(ButtonState.Available);
    }

    public char GetNextChar(int charIndex)
    {
        if (charIndex >= BaseButtonText.Length)
            return '@';

        if (charIndex > _highlightedCharCount)
            return '@';

        return char.ToUpper(BaseButtonText[charIndex]);
    }

    public string PressButton()
    {
        ChangeButtonState(ButtonState.Pressed);
        return BaseButtonText;
    }

    public void ToggleAvailable(bool toggleON, bool forceDisable)
    {
        if (forceDisable)
        {
            ChangeButtonState(ButtonState.Unavailable);
            _buttonTextMesh.text = BaseButtonText;
        }

        if (CurrentState != ButtonState.Pressed)
        {
            if (toggleON)
            {
                ChangeButtonState(ButtonState.Available);
            }
            else
            {
                ChangeButtonState(ButtonState.Unavailable);
            }
        }
    }

    protected void ChangeButtonState(ButtonState newState)
    {
        if (newState == CurrentState)
            return;

        CurrentState = newState;

        switch (newState)
        {
            case ButtonState.Unavailable:
                _animator.SetBool("IsAvailable", false);
                _animator.SetBool("IsSelected", false);
                _animator.SetBool("IsReady", false);
                _animator.SetBool("IsPressed", false);
                break;

            case ButtonState.Available:
                _animator.SetBool("IsAvailable", true);
                _animator.SetBool("IsSelected", false);
                _animator.SetBool("IsReady", false);
                _animator.SetBool("IsPressed", false);
                break;

            case ButtonState.Selected:
                _animator.SetBool("IsAvailable", true);
                _animator.SetBool("IsSelected", true);
                _animator.SetBool("IsReady", false);
                _animator.SetBool("IsPressed", false);
                break;

            case ButtonState.Ready:
                _animator.SetBool("IsAvailable", true);
                _animator.SetBool("IsSelected", true);
                _animator.SetBool("IsReady", true);
                _animator.SetBool("IsPressed", false);
                break;

            case ButtonState.Pressed:
                _animator.SetBool("IsAvailable", true);
                _animator.SetBool("IsSelected", true);
                _animator.SetBool("IsReady", true);
                _animator.SetBool("IsPressed", true);
                break;
            default:
                break;
        }
    }
}

public enum ButtonState
{
    Unavailable,
    Available,
    Selected,
    Ready,
    Pressed
}
