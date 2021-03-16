using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WritingMachineController : MonoBehaviour
{
    private SlidableWritingMachine _slidableMachine;
    private KeyPadController _keyPadController;

    private List<ButtonController> _allButtons;
    private List<ButtonController> _currentAvailableButtons = new List<ButtonController>();
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

        _allButtons = new List<ButtonController>(GetComponentsInChildren<ButtonController>());
        _buttonCount = _allButtons.Count;

        SpawnCommandDocument();
    }

    private void Start()
    {
        commandButtonSection.AssignButtonPositions(allCommandButtons);
        shipButtonSection.AssignButtonPositions(allShipButtons);
    }

    private void OnEnable()
    {
        Ship.newShipAvailable += OnNewShipAvailable;
        Ship.shipUnavailable += OnShipUnavailable;
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipUnavailable -= OnShipUnavailable;
    }

    public void OnMachineOpen()
    {
        ChangeButtonSection(ButtonSectionType.Commands);
    }

    public void OnMachineClose()
    {
        DisableAllButtons();
        _keyPadController.CloseKeyPad();
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

        if (_currentButtonSectionType == ButtonSectionType.KeyPadVector || _currentButtonSectionType == ButtonSectionType.KeyPadCode)
        {
            HandleKeyPadInput(playerInput);
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

    private void HandleKeyPadInput(string playerInput)
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
            ChangeButtonSection(VerifyNextSection());
            // Debug.Log(buttonText);

            _currentCharIndex = 0;
            _currentString = "";
        }
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
                printText = "Product code : " + keyPadEntry.ToString();
                _currentCommandDocument.AssignTargetGridCoords(keyPadEntry, printText);
                break;
        }
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
                break;

            case ButtonSectionType.Ships:
                _currentAvailableButtons = allShipButtons;
                _keyPadController.CloseKeyPad();
                break;

            case ButtonSectionType.KeyPadVector:
                _currentAvailableButtons = null;
                _keyPadController.OpenKeyPad();
                break;

            case ButtonSectionType.KeyPadCode:
                _currentAvailableButtons = null;
                _keyPadController.OpenKeyPad();
                break;

            case ButtonSectionType.End:
                _currentAvailableButtons = null;
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
                if (_openShipButtons)
                    return ButtonSectionType.Ships;

                else if (_openKeyPadVector)
                    return ButtonSectionType.KeyPadVector;

                else if (_openKeyPadCode)
                    return ButtonSectionType.KeyPadCode;

                else
                    return ButtonSectionType.End;

            case ButtonSectionType.Ships:
                Debug.Log("YO1");
                Debug.Log(_currentCommandButton.gameObject.name);
                Debug.Log(_currentCommandButton.openKeyPadVector);
                if (_currentCommandButton.openKeyPadVector)
                {
                    Debug.Log("YO2");
                    return ButtonSectionType.KeyPadVector;
                }
                else
                    return ButtonSectionType.End;
        }

        return ButtonSectionType.End;
    }

    private void DisableAllButtons()
    {
        foreach (var btn in _allButtons)
        {
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
        
        yield return new WaitForSeconds(0.05f);
        _keyPadController.CloseKeyPad();
        yield return new WaitForSeconds(0.45f);
        DisableAllButtons();
        ChangeButtonSection(ButtonSectionType.Commands);
    }

    private void SpawnCommandDocument()
    {
        _currentCommandDocument = Instantiate(commandPrefab, transform).GetComponent<MovableCommand>();
        _currentCommandDocument.transform.position = commandSpawnPos.position;
        _currentCommandDocument.commandTeared += OnCommandTeared;
    }

    private void OnCommandTeared()
    {
        _currentCommandDocument.commandTeared -= OnCommandTeared;
        _currentCommandDocument.transform.parent = null;
        _currentCommandDocument = null;

        SpawnCommandDocument();
    }
}
