using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineController : MonoBehaviour
{
    public static Action<WritingMachineController> writingMachineOpen;
    public static Action<bool> commandReadyToTear;
    public static Action commandTeared;
    public static Action<bool> lightFlash;

    private SlidableWritingMachine _slidableMachine;
    private KeyPadController _keyPadController;
    private RouteScreenController _routeScreenController;
    private WritingMachineKeyboard _keyboard;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private List<ButtonController> _allButtons;
    private List<ButtonController> _currentAvailableButtons = new List<ButtonController>();
    private List<ButtonController> _currentPressedButtons = new List<ButtonController>();
    private int _currentAvailableButtonsCount;

    private int _buttonCount;
    private string _currentString = "";
    private int _currentCharIndex;

    [Header("Base settings")]
    public bool generateStartCommand;

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
    public MovableCommand CurrentCommandDocument { get { return _currentCommandDocument; } }
    public int commandSpawnSortOrder;

    private ButtonController_Command _currentCommandButton;
    private bool _openShipButtons;
    private bool _openKeyPadVector;
    private bool _openKeyPadCode;

    private void Awake()
    {
        _slidableMachine = GetComponent<SlidableWritingMachine>();
        _keyPadController = GetComponentInChildren<KeyPadController>();
        _routeScreenController = GetComponentInChildren<RouteScreenController>();
        _keyboard = GetComponentInChildren<WritingMachineKeyboard>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _allButtons = new List<ButtonController>(GetComponentsInChildren<ButtonController>());
        _buttonCount = _allButtons.Count;

        SpawnCommandDocument();
    }

    private void Start()
    {
        commandButtonSection.AssignCommandDisplaysPositions(allCommandButtons);
        shipButtonSection.AssignButtonPositions(allShipButtons);

        _currentButtonSectionType = ButtonSectionType.Ships;

        if (generateStartCommand)
        {
            AssignStartCommand();
            ChangeButtonSection(ButtonSectionType.End);
        }
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

        if (writingMachineOpen != null)
            writingMachineOpen(this);
    }

    public void OnMachineClose()
    {
        DisableAllButtons(true);
        if (_keyPadController != null)
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
                _keyboard.PressKey(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            RemoveCharacter();
            _keyboard.PressKey(SpecialKey.Backspace);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            PressReadyButton();
            _keyboard.PressKey(SpecialKey.Return);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _keyboard.PressKey(SpecialKey.Tab);
        }
    }

    private void HandleKeyPadVectorInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _keyPadController.OnCharInput(playerInput[i]);
                _keyboard.PressKey(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _keyPadController.OnBackspaceInput();
            _keyboard.PressKey(SpecialKey.Backspace);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Vector2 keypadVector = _keyPadController.GetVectorValue();
            //Debug.Log(keypadVector);

            AssignButtonFields(keypadVector);

            _keyPadController.OnEnterInput();
            _keyboard.PressKey(SpecialKey.Return);
            ChangeButtonSection(VerifyNextSection());
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _keyboard.PressKey(SpecialKey.Tab);
        }
    }

    private void HandleKeyPadCodeInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _keyPadController.OnCharInput(playerInput[i]);
                _keyboard.PressKey(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _keyPadController.OnBackspaceInput();
            _keyboard.PressKey(SpecialKey.Backspace);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            int keypadCode = _keyPadController.GetNumberValue();
            //Debug.Log(keypadVector);
            
            AssignButtonFields(keypadCode);

            _keyPadController.OnEnterInput();
            _keyboard.PressKey(SpecialKey.Return);
            ChangeButtonSection(VerifyNextSection());
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _keyboard.PressKey(SpecialKey.Tab);
        }
    }

    private void HandleRouteInput(string playerInput)
    {
        if (playerInput.Length > 0)
        {
            for (int i = 0; i < playerInput.Length; i++)
            {
                _routeScreenController.OnCharInput(playerInput[i]);
                _keyboard.PressKey(playerInput[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            _routeScreenController.OnBackspaceInput();
            _keyboard.PressKey(SpecialKey.Backspace);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _routeScreenController.OnTabInput();
            _keyboard.PressKey(SpecialKey.Tab);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            List<string> routeInput = _routeScreenController.GetRouteInput();
            //Debug.Log(keypadVector);

            AssignButtonFields(routeInput);
            _routeScreenController.OnEnterInput();
            _keyboard.PressKey(SpecialKey.Return);
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
        if (_currentString == "")
            return;

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
                if (i == 1)
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
        //Debug.Log("new button section : " + nextSection);

        switch (_currentButtonSectionType)
        {
            case ButtonSectionType.Commands:
                _currentAvailableButtons = allCommandButtons;
                if (_keyPadController != null)
                    _keyPadController.CloseKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                _animator.SetInteger("ButtonSection", 2);
                break;

            case ButtonSectionType.Ships:
                _currentAvailableButtons = allShipButtons;
                if (_keyPadController != null)
                    _keyPadController.CloseKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                _animator.SetInteger("ButtonSection", 1);
                break;

            case ButtonSectionType.KeyPadVector:
                _currentAvailableButtons = null;
                if (_keyPadController != null)
                    _keyPadController.OpenKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                _animator.SetInteger("ButtonSection", 9);
                break;

            case ButtonSectionType.KeyPadCode:
                _currentAvailableButtons = null;
                if (_keyPadController != null)
                    _keyPadController.OpenKeyPad();
                _routeScreenController.CloseRouteScreen(true);
                _animator.SetInteger("ButtonSection", 4);
                break;

            case ButtonSectionType.RouteScreen:
                _currentAvailableButtons = null;
                _routeScreenController.OpenRouteScreen();
                _animator.SetInteger("ButtonSection", 3);
                break;

            case ButtonSectionType.End:
                _currentAvailableButtons = null;
                _routeScreenController.CloseRouteScreen(true);
                _animator.SetInteger("ButtonSection", 0);
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
                if (_currentCommandButton != null)
                {
                    if (_currentCommandButton.openKeyPadCode)
                        return ButtonSectionType.KeyPadCode;

                    else if (_currentCommandButton.openRouteScreen)
                        return ButtonSectionType.RouteScreen;
                }
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

        if (_keyPadController != null)
            _keyPadController.CloseKeyPad();
        _routeScreenController.CloseRouteScreen(true);

        bool openDrawer = _currentCommandDocument.openDrawer;

         if (commandReadyToTear != null)
            commandReadyToTear(openDrawer);

        yield return new WaitForSeconds(0.45f);
        DisableAllButtons();
        //ChangeButtonSection(ButtonSectionType.Commands);
    }

    private void SpawnCommandDocument()
    {
        _currentCommandDocument = Instantiate(commandPrefab, transform).GetComponent<MovableCommand>();
        _currentCommandDocument.transform.position = commandSpawnPos.position;
        _currentCommandDocument.SetSortingLayer(_spriteRenderer.sortingLayerID);
        _currentCommandDocument.SetOrderInLayer(commandSpawnSortOrder);
        _currentCommandDocument.commandTeared += OnCommandTeared;
    }

    private void OnCommandTearStart()
    {
        ChangeButtonSection(ButtonSectionType.End);
    }

    private void OnCommandTeared(MovableCommand cmd)
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

    private void AssignStartCommand()
    {
        _currentCommandDocument.gameObject.tag = "Indestruc";

        string startMessage = "<color=red><i>TEAR THIS DOCUMENT</color></i>";
        PrintText(startMessage, false, false, "StartLevel");
        startMessage = "Welcome, trainee #01235!";
        PrintText(startMessage, false, false, "StartLevel");
        startMessage = "When ready to start day, sign-in by sending this document through your outbox.";
        PrintText(startMessage, false, false, "StartLevel");
    }

    public void PrintText(string message, bool shreddable, bool openDrawer, string commandName = "")
    {
        if (shreddable)
            _currentCommandDocument.gameObject.tag = "Destruc";
        else
        _currentCommandDocument.gameObject.tag = "Indestruc";

        _currentCommandDocument.openDrawer = openDrawer;
        _currentCommandDocument.AssignCommandName(commandName, message);
        ChangeButtonSection(ButtonSectionType.End);
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

    public void TriggerClickSound(int lightOn)
    {
        bool on = lightOn == 1 ? true : false;

        if (lightFlash != null)
            lightFlash(on);
    }
}
