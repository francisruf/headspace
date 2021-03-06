﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class ButtonController : MonoBehaviour
{
    public static Action buttonPress;

    protected TextMeshProUGUI _buttonTextMesh;
    protected Animator _animator;
    public string BaseButtonText { get; protected set; }
    private string _temp;
    private string _highlightedText
    {
        get
        {
            return _temp;
        }
        set
        {
            _temp = value;
        }
    }

    public string printText;
    public bool printButtonName;

    protected int _highlightedCharCount;
    public ButtonState CurrentState { get; protected set; }

    public ButtonSectionType buttonType;
    public ButtonSectionType nextButtonSection;

    public Color highlightColor;

    private void Awake()
    {
        _buttonTextMesh = GetComponentInChildren<TextMeshProUGUI>();
        _animator = GetComponent<Animator>();
        _highlightedText = BaseButtonText;
    }

    protected virtual void Start()
    {
        if (_buttonTextMesh != null)
            BaseButtonText = _buttonTextMesh.text;

        ChangeButtonState(ButtonState.Unavailable);
    }

    public virtual string GetButtonPrintText()
    {
        if (printButtonName)
        {
            return printText + "<b>" + BaseButtonText + "</b>";
        }
        return printText;
    }

    public virtual string GetButtonCommandField()
    {
        return BaseButtonText;
    }

    public virtual void HighlightCharacter(int charIndex)
    {
        if (charIndex > _highlightedCharCount)
            return;
        //EEDF84
        _highlightedText = "<color=#" + ColorUtility.ToHtmlStringRGB(highlightColor) + ">";
        int charCount = 0;

        for (int i = 0; i <= charIndex; i++)
        {
            _highlightedText += BaseButtonText[i];
            charCount++;
        }

        _highlightedText += "</color>";

        for (int i = charCount; i < BaseButtonText.Length; i++)
        {
            _highlightedText += BaseButtonText[i];
        }

        _highlightedCharCount++;
        _buttonTextMesh.text = _highlightedText;

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

    public virtual void ClearHighlighting()
    {
        if (_buttonTextMesh != null)
        {
            _buttonTextMesh.text = BaseButtonText;
        }

        _highlightedCharCount = 0;
        _highlightedText = BaseButtonText;
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
        if (buttonPress != null)
            buttonPress();

        ChangeButtonState(ButtonState.Pressed);
        return BaseButtonText;
    }

    public virtual void ToggleAvailable(bool toggleON, bool forceDisable)
    {
        if (forceDisable)
        {
            ChangeButtonState(ButtonState.Unavailable);
            if (_buttonTextMesh != null)
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
        else
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

        if (_buttonTextMesh != null)
        {
            if (toggleON)
            {
                _buttonTextMesh.text = _highlightedText;
            }
            else
                _buttonTextMesh.text = BaseButtonText;
        }

    }

    //public virtual void OnSelectedButtonMachineClose()
    //{
    //    if (_buttonTextMesh != null)
    //        _buttonTextMesh.text = BaseButtonText;
    //}

    public virtual void ChangeButtonState(ButtonState newState)
    {
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
