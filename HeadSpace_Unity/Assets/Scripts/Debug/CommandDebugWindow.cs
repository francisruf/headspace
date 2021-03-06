using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CommandDebugWindow : MonoBehaviour
{
    // Action qui envoie les strings au CommandManager
    public static Action<string, string, string> newCommandRequest;

    // Input fields
    public TMP_InputField commandInputField;
    public TMP_InputField shipInputField;
    public TMP_InputField coordsInputField;
    public Button confirmButton;

    // Instructions text
    public TextMeshProUGUI commandDebugText;

    // Fonction qui sélectionne le premier champ lorsque la fenêtre est ouverte
    private void OnEnable()
    {
        commandDebugText.enabled = false;
        StartCoroutine(SelectTimer());
    }

    private IEnumerator SelectTimer()
    {
        yield return new WaitForEndOfFrame();
        commandInputField.Select();
    }

    // Fonction qui RESET tous les champs textuels à chaque fois que la fenêtre est disabled
    private void OnDisable()
    {
        commandInputField.text = "";
        shipInputField.text = "";
        coordsInputField.text = "";
        commandDebugText.enabled = true;
    }

    // Fonction qui prend les textes dans les input fields et déclenche l'action qui envoie la commande
    public void Confirm()
    {
        string commandText = commandInputField.text;
        string shipText = shipInputField.text;
        string coordsText = coordsInputField.text;

        if (newCommandRequest != null)
            newCommandRequest(commandText, shipText, coordsText);

        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Confirmer la commande avec un SUPER SHORTCUT YOOHOO
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Confirm();
        }

        // Naviguer entre les différents éléments de UI avec TAB
        // La fonction "Select" est utilisée avec le EventSystem, qui détermine quel élément de UI (Selectable) est sélectionné
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == commandInputField.gameObject)
            {
                shipInputField.Select();
            }
            else if (EventSystem.current.currentSelectedGameObject == shipInputField.gameObject)
            {
                coordsInputField.Select();
            }
            else if (EventSystem.current.currentSelectedGameObject == coordsInputField.gameObject)
            {
                confirmButton.Select();
            }
            else if (EventSystem.current.currentSelectedGameObject == confirmButton.gameObject)
            {
                commandInputField.Select();
            }
        }
    }
}
