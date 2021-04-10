﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineController : MonoBehaviour
{
    public static Action commandReadyToTear;
    public static Action commandTeared;

    private SlidableWritingMachine _slidableMachine;
    private KeyPadController _keyPadController;
    private RouteScreenController _routeScreenController;
    private Animator _animator;

    private List<ButtonController> _allButtons;
    private List<ButtonController> _currentAvailableButtons = new List<ButtonController>();
    private List<ButtonController> _currentPressedButtons = new List<ButtonController>();
    private int _currentAvailableButtonsCount;

    private int _buttonCount;
    private string _currentString = "";
    private int _currentCharIndex;

    [Header("Assign buttons here")]
    public List<ButtonController> allCommandButtons;
    private List<ButtonController> allShipButtons = new List<ButtonController>();

    [Header("Button sections")]
    public ButtonSection commandButtonSection;
    public ButtonSection shipButtonSection;
    private ButtonSection _currentButtonSection;
    private ButtonSectionType _currentButtonSectionType;

    [Header("Button prefabs")]
    public GameObject shipButtonPrefab;

    [Header("Command prefab")]
    public GameObject commandPrefab;
    public Transform commandSpawnPos;
    private MovableCommand _currentCommandDocument;

    private ButtonController_Command _currentCommandButton;
    private bool _openShipButtons;
    private bool _openKeyPadVector;
    private bool _openKeyPadCode;

    private void Awake()
    {
        _slidableMachine = GetComponent<SlidableWritingMachine>();
        _keyPadController = GetComponentInChildren<KeyPadController>();
        _routeScreenController = GetComponentInChildren<RouteScreenController>();
        _animator = GetComponent<Animator>();

        _allButtons = new List<ButtonController>(GetComponentsInChildren<ButtonController>());
        _buttonCount = _allButtons.Count;

        SpawnCommandDocument();
    }

    private void Start()
    {
        commandButtonSection.AssignButtonPositions(allCommandButtons);
        shipButtonSection.AssignButtonPositions(allShipButtons);

        _currentButtonSectionType = ButtonSectionType.Ships;
    }

    private void OnEnable()
    {
        Ship.newShipAvailable += OnNewShipAvailable;
        Ship.shipUnavailable += OnShipUnavailable;
        Ship.shipInfoChange += OnShipInfoChange;
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipUnavailable -= OnShipUnavailable;
        Ship.shipInfoChange -= OnShipInfoChange;
    }

    public void OnMachineOpen()
    {
        ChangeButtonSection(_currentButtonSectionType);

        if (_currentButtonSectionType != ButtonSectionType.End)
        {
            foreach (var btn in _currentPressedButtons)
            {
                btn.PressButton();
            }
        }

        _animator.SetBool("IsOpen", true);
    }

    public void OnMachineClose()
    {
        DisableAllButtons(true);
        _keyPadController.CloseKeyPad();
        _routeScreenController.CloseRouteScreen(false);
        _currentString = "";
        _currentCharIndex = 0;

        _animator.SetBool("IsOpen", false);
    }

    private void OnNewShipAvailable(Ship ship)
    {
        ButtonController_Ship newShipButton = Instantiate(shipButtonPrefab).GetComponent<ButtonController_Ship>();
        newShipButton.InitializeShipButton(ship);
        shipButtonSection.AssignToNextAvailableSlot(newShipButton);
        _allButtons.Add(newShipButton);
        allShipButtons.Add(newShipButton);
        UpdateButtonCount();
    }

    private void OnShipUnavailable(Ship ship)
    {

    }

    private void Update()
    {
        if (!_slidableMachine.IsOpen)
            return;

        string playerInput = Input.inputString;

        if (_currentButtonSectionType == ButtonSectionType.KeyPadVector)
        {
            HandleKeyPadVectorInput(playerInput);
        }
        else if (_currentButtonSectionType == ButtonSectionType.KeyPadCode)
        {
            HandleKeyPadCodeInput(playerInput);
        }
        else if (_currentButtonSectionType == ButtonSectionType.RouteScreen)
        {
            HandleRouteInput(playerInput);
        }
        else
        {
            HandleButtonsInput(playerInput);
        }

    }

    private void HandleButtonsInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                LookForChar(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveCharacter();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PressReadyButton();
        }
    }

    private void HandleKeyPadVectorInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _keyPadController.OnCharInput(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _keyPadController.OnBackspaceInput();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Vector2 keypadVector = _keyPadController.GetVectorValue();
            //Debug.Log(keypadVector);

            AssignButtonFields(keypadVector);

            _keyPadController.OnEnterInput();
            ChangeButtonSection(VerifyNextSection());
        }
    }

    private void HandleKeyPadCodeInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _keyPadController.OnCharInput(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _keyPadController.OnBackspaceInput();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            int keypadCode = _keyPadController.GetNumberValue();
            //Debug.Log(keypadVector);
            
            AssignButtonFields(keypadCode);

            _keyPadController.OnEnterInput();
            ChangeButtonSection(VerifyNextSection());
        }
    }

    private void HandleRouteInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _routeScreenController.OnCharInput(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _routeScreenController.OnBackspaceInput();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _routeScreenController.OnTabInput();
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            List<string> routeInput = _routeScreenController.GetRouteInput();
            //Debug.Log(keypadVector);

            AssignButtonFields(routeInput);
            _routeScreenController.OnEnterInput();
            ChangeButtonSection(VerifyNextSection());
        }
    }

    private void LookForChar(char c)
    {
        c = char.ToUpper(c);
        bool foundMatch = false;
        List<ButtonController> _matchingButtons = new List<ButtonController>();

        for (int i = 0; i < _currentAvailableButtonsCount; i++)
        {
            if (_currentAvailableButtons[i].GetNextChar(_currentCharIndex) == c)
            {
                _currentAvailableButtons[i].HighlightCharacter(_currentCharIndex);
                _matchingButtons.Add(_currentAvailableButtons[i]);
                foundMatch = true;
            }
        }

        if (foundMatch)
        {
            _currentCharIndex++;
            _currentString += c;

            for (int i = 0; i < _currentAvailableButtonsCount; i++)
            {
                if (!_matchingButtons.Contains(_currentAvailableButtons[i]))
                {
                    _currentAvailableButtons[i].ClearHighlighting();
                }
            }
        }
    }

    private void RemoveCharacter()
    {
        if (_currentString.Length <= 0)
            return;

        string newString = _currentString.Substring(0,_currentCharIndex-1);
        _currentString = "";
        _currentCharIndex = 0;

        for (int i = 0; i < _currentAvailableButtonsCount; i++)
        {
            _currentAvailableButtons[i].ClearHighlighting();
        }

        for (int i = 0; i < newString.Length; i++)
        {
            LookForChar(newString[i]);
        }
    }

    private void PressReadyButton()
    {
        ButtonController candidateButton = null;

        for (int i = 0; i < _currentAvailableButtonsCount; i++)
        {
            if (_currentAvailableButtons[i].CurrentState == ButtonState.Ready)
            {
                candidateButton = _currentAvailableButtons[i];
                break;
            }
        }

        if (candidateButton != null)
        {
            _currentPressedButtons.Add(candidateButton);

            ButtonController_Command cmdButton = candidateButton.GetComponent<ButtonController_Command>();
            if (cmdButton != null)
            {
                _currentCommandButton = cmdButton;
                _openShipButtons = cmdButton.openShipButtons;
                _openKeyPadVector = cmdButton.openKeyPadVector;
                _openKeyPadCode = cmdButton.openKeyPadCode;
            }

            string buttonText = candidateButton.PressButton();
            AssignButtonFields(candidateButton);
        }
        else if (_currentString != "")
        {
            AssignCommandErrorMessage(_currentString);
        }
        else
        {
            _currentCommandDocument.AssignErrorMessage("-- General directive --");
        }

        ChangeButtonSection(VerifyNextSection());
        _currentCharIndex = 0;
        _currentString = "";
    }

    private void AssignCommandErrorMessage(string playerInput)
    {
        _currentCommandDocument.AssignErrorMessage("ERROR : Invalid input - " + playerInput);
    }

    private void AssignButtonFields(ButtonController currentButton)
    {
        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.Commands:
                _currentCommandDocument.AssignCommandName(currentButton.GetButtonCommandField(), currentButton.GetButtonPrintText());
                break;
            case ButtonSectionType.Ships:
                _currentCommandDocument.AssignShipName(currentButton.GetButtonCommandField(), currentButton.GetButtonPrintText());
                break;
            case ButtonSectionType.End:
                break;
        }
    }

    private void AssignButtonFields(int keyPadEntry)
    {
        string printText = "";

        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.KeyPadCode:

                string stringCode = keyPadEntry.ToString();
                //string actualCode = "";
                //for (int i = 0; i < stringCode.Length; i++)
                //{
                //    if (stringCode[i] < 48 || stringCode[i] > 57)
                //    {

                //    }
                //    else
                //    {
                //        actualCode += stringCode[i];
                //    }
                //}

                printText = "Product code : " + stringCode;
                _currentCommandDocument.AssignProductCode(stringCode, printText);
                break;
        }
    }

    private void AssignButtonFields(Vector2 keyPadEntry)
    {
        string printText = "";

        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.KeyPadVector:
                printText = "To coordinates : " + keyPadEntry.ToString();
                _currentCommandDocument.AssignTargetGridCoords(keyPadEntry, printText);
                break;
            case ButtonSectionType.KeyPadCode:

                string stringCode = keyPadEntry.ToString();
                string actualCode = "";
                for (int i = 0; i < stringCode.Length; i++)
                {
                    if (stringCode[i] < 48 || stringCode[i] > 57)
                    {
                        
                    }
                    else
                    {
                        actualCode += stringCode[i];
                    }
                }

                printText = "Product code : " + actualCode;
                _currentCommandDocument.AssignProductCode(actualCode, printText);
                break;
        }
    }

    private void AssignButtonFields(List<string> routeEntry)
    {
        if (routeEntry == null)
            return;

        int count = routeEntry.Count;
        if (count <= 0)
            return;

        string printText = "";

        if (count == 1)
            printText = "Towards this destination : " + routeEntry[0] + ".";
        else
        {
            printText = "Following this route : ";
            for (int i = 0; i < count; i++)
            {
                if (i == 3)
                    printText += "\n";

                printText += routeEntry[i];
                if (i < count - 1)
                    printText += " - ";
                else
                    printText += ".";
            }
        }
        _currentCommandDocument.AssignRoute(routeEntry, printText);
    }

    private void ChangeButtonSection(ButtonSectionType nextSection)
    {
        if (_currentAvailableButtonsCount > 0)
        {
            for (int i = 0; i < _currentAvailableButtonsCount; i++)
            {
                _currentAvailableButtons[i].ToggleAvailable(false, false);
            }
            _currentAvailableButtonsCount = 0;
        }

        _currentButtonSectionType = nextSection;
        Debug.Log("new button section : " + nextSection);

        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.Commands:
                _currentAvailableButtons = allCommandButtons;
                _keyPadController.CloseKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                break;

            case ButtonSectionType.Ships:
                _currentAvailableButtons = allShipButtons;
                _keyPadController.CloseKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                break;

            case ButtonSectionType.KeyPadVector:
                _currentAvailableButtons = null;
                _keyPadController.OpenKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                break;

            case ButtonSectionType.KeyPadCode:
                _currentAvailableButtons = null;
                _keyPadController.OpenKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                break;

            case ButtonSectionType.RouteScreen:
                _currentAvailableButtons = null;
                _routeScreenController.OpenRouteScreen();
                break;

            case ButtonSectionType.End:
                _currentAvailableButtons = null;
                _routeScreenController.CloseRouteScreen(true);
                StartCoroutine(EndCommand());
                break;

            default:
                break;
        }

        UpdateButtonCount();

        if (_currentAvailableButtonsCount > 0)
        {
            for (int i = 0; i < _currentAvailableButtonsCount; i++)
            {
                _currentAvailableButtons[i].ToggleAvailable(true, false);
            }
        }
        //else
        //{
        //    if (_currentButtonSectionType != ButtonSectionType.KeyPad)
        //    {
        //        DisableAllButtons();
        //    }
        //}
    }

    private ButtonSectionType VerifyNextSection()
    {
        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.Commands:
                if (_currentCommandButton.openKeyPadCode)
                    return ButtonSectionType.KeyPadCode;

                else if (_currentCommandButton.openRouteScreen)
                    return ButtonSectionType.RouteScreen;

                else
                    return ButtonSectionType.End;

            case ButtonSectionType.Ships:
                return ButtonSectionType.Commands;
        }
        return ButtonSectionType.End;
    }

    private void DisableAllButtons(bool forceReset = false)
    {
        foreach (var btn in _allButtons)
        {
            if (forceReset)
                btn.ClearHighlighting();

            btn.ToggleAvailable(false, true);
        }
    }

    private void UpdateButtonCount()
    {
        if (_currentAvailableButtons != null)
        {
            _currentAvailableButtonsCount = _currentAvailableButtons.Count;
        }
        else
        {
            _currentAvailableButtonsCount = 0;
        }
    }

    private IEnumerator EndCommand()
    {
        _animator.SetBool("IsReady", true);

        yield return new WaitForSeconds(0.05f);
        _keyPadController.CloseKeyPad();
        _routeScreenController.CloseRouteScreen(true);

        if (commandReadyToTear != null)
            commandReadyToTear();

        yield return new WaitForSeconds(0.45f);
        DisableAllButtons();
        //ChangeButtonSection(ButtonSectionType.Commands);
    }

    private void SpawnCommandDocument()
    {
        _currentCommandDocument = Instantiate(commandPrefab, transform).GetComponent<MovableCommand>();
        _currentCommandDocument.transform.position = commandSpawnPos.position;
        _currentCommandDocument.commandTeared += OnCommandTeared;
    }

    private void OnCommandTearStart()
    {
        ChangeButtonSection(ButtonSectionType.End);
    }

    private void OnCommandTeared()
    {
        _currentCommandDocument.commandTeared -= OnCommandTeared;
        _currentCommandDocument.transform.parent = null;
        _currentCommandDocument = null;
        _currentPressedButtons.Clear();
        DisableAllButtons(true);

        _currentCharIndex = 0;
        _currentString = "";

        _animator.SetBool("IsReady", false);
        if (commandTeared != null)
            commandTeared();

        ChangeButtonSection(ButtonSectionType.Ships);
        SpawnCommandDocument();
    }

    private void OnShipInfoChange(Ship ship)
    {
        if (_currentButtonSectionType == ButtonSectionType.Ships)
        {
            _currentString = "";
            _currentCharIndex = 0;

            for (int i = 0; i < _currentAvailableButtonsCount; i++)
            {
                _currentAvailableButtons[i].ClearHighlighting();
            }
        }
    }
}
