using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KeyPadController : MonoBehaviour
{
    public static Action keypadOpen;
    public static Action keypadClose;

    public Animator animator;
    private KeyPadButtonController[] _allKeyPadButtons;
    public TextMeshProUGUI[] allVectorTexts = new TextMeshProUGUI[4];
    private int _currentTextIndex;
    private int _valuesEntered;
    private int[] _vectorValues = new int[4];

    private IEnumerator _currentAnimRoutine;
    private bool _isOpen;
    private bool _isComplete;

    private void Awake()
    {
        _allKeyPadButtons = GetComponentsInChildren<KeyPadButtonController>();
    }

    private void Start()
    {
        for (int i = 0; i < allVectorTexts.Length; i++)
        {
            allVectorTexts[i].text = "0";
        }
    }

    public void OnCharInput(char playerCharInput)
    {
        NewPlayerInput(playerCharInput);
    }

    public void OnEnterInput()
    {
        _allKeyPadButtons[_allKeyPadButtons.Length - 1].Press();
    }

    public void OnBackspaceInput()
    {
        ResetLastValue();
    }

    public void OpenKeyPad()
    {
        if (_isOpen)
        {
            ResetAllValues();
        }
        else
        {
            if (_currentAnimRoutine == null)
            {
                _currentAnimRoutine = RevealKeyPad();
                StartCoroutine(_currentAnimRoutine);
            }
        }
    }

    private IEnumerator RevealKeyPad()
    {
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.15f);

        if (keypadOpen != null)
            keypadOpen();

        yield return new WaitForSeconds(0.15f);

        _isOpen = true;
        ToggleButtons(true);

        _currentAnimRoutine = null;
    }

    public void CloseKeyPad()
    {
        if (!_isOpen)
        {
            return;
        }
        else
        {
            //Debug.Log("CLOSE CALLED");
            if (_currentAnimRoutine == null)
            {
                _currentAnimRoutine = HideKeyPad();
                StartCoroutine(_currentAnimRoutine);
            }
        }
    }

    private IEnumerator HideKeyPad()
    {
        yield return new WaitForSeconds(0.1f);

        if (keypadClose != null)
            keypadClose();

        ToggleButtons(false);
        _isOpen = false;

        animator.SetBool("IsOpen", false);

        yield return new WaitForSeconds(0.4f);
        _currentAnimRoutine = null;
    }

    private void ToggleButtons(bool toggleOn)
    {
        for (int i = 0; i < _allKeyPadButtons.Length; i++)
        {
            _allKeyPadButtons[i].ToggleAvailable(toggleOn);
        }

        ResetAllValues();
    }

    public void NewPlayerInput(char inputChar)
    {
        // Accepter seulement les inputs de 0 à 9
        if (inputChar < 48 || inputChar > 57)
            return;

        int targetButtonIndex = inputChar - 48;
        _allKeyPadButtons[targetButtonIndex].Press();

        UpdateValue(inputChar);
    }

    private void UpdateValue(char c)
    {
        _vectorValues[_currentTextIndex] = int.Parse(c.ToString());
        _currentTextIndex = Mathf.Clamp(_currentTextIndex + 1, 0, 3);
        _valuesEntered = Mathf.Clamp(_valuesEntered + 1, 0, 4);

        if(_valuesEntered + 1 > 4)
        {
            _isComplete = true;
        }

        UpdateVectorTexts();
    }

    private void ResetLastValue()
    {
        if (_isComplete)
        {
            _isComplete = false;
        }
        else
        {
            _currentTextIndex = Mathf.Clamp(_currentTextIndex - 1, 0, 3);
        }

        _valuesEntered = Mathf.Clamp(_valuesEntered - 1, 0, 4);
        _vectorValues[_currentTextIndex] = 0;
        UpdateVectorTexts();
    }

    private void ResetAllValues()
    {
        _currentTextIndex = 0;
        _valuesEntered = 0;

        for (int i = 0; i < 4; i++)
        {
            _vectorValues[i] = 0;
            allVectorTexts[i].text = "0";
        }

        _isComplete = false;
        UpdateVectorTexts();
    }

    private void UpdateVectorTexts()
    {
        if (!_isOpen)
        {
            for (int i = 0; i < 4; i++)
            {
                allVectorTexts[i].text = _vectorValues[i].ToString();
            }
            return;
        }

        if (!_isComplete)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == _currentTextIndex)
                {
                    allVectorTexts[i].text = "<color=#EEDF84>" + _vectorValues[i] + "</color>";
                }
                else
                {
                    allVectorTexts[i].text = _vectorValues[i].ToString();
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                allVectorTexts[i].text = "<color=#EEDF84>" + _vectorValues[i] + "</color>";
            }
        }
    }

    public Vector2 GetVectorValue()
    {
        float x = _vectorValues[0];
        x += _vectorValues[1] / 10f;
        float y = _vectorValues[2];
        y += _vectorValues[3] / 10f;

        return new Vector2(x, y);
    }
}
