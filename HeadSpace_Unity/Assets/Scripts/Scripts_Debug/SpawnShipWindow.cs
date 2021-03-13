using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SpawnShipWindow : MonoBehaviour
{
    public TMP_InputField shipNameField;
    public TMP_InputField shipCallsignField;
    public Button confirmButton;

    public GameObject shipPrefab;

    public void Confirm()
    {
        string shipName = shipNameField.text;
        string shipCallsign = shipCallsignField.text;

        if (shipName.Length <= 0)
        {
            Debug.Log("INVALID SHIP NAME");
            this.gameObject.SetActive(false);
            return;
        }
        else if (shipCallsign.Length != 3)
        {
            Debug.Log("INVALID CALLSIGN");
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            Ship newShip = Instantiate(shipPrefab).GetComponent<Ship>();
            newShip.InitializeShip(shipName, shipCallsign, ShipState.AtBase);
            this.gameObject.SetActive(false);
        }
    }

    // Fonction qui RESET tous les champs textuels à chaque fois que la fenêtre est disabled
    // Fonction qui sélectionne le premier champ lorsque la fenêtre est ouverte
    private void OnEnable()
    {
        StartCoroutine(SelectTimer());
    }

    private IEnumerator SelectTimer()
    {
        yield return new WaitForEndOfFrame();
        shipNameField.Select();
    }

    private void OnDisable()
    {
        shipNameField.text = "";
        shipCallsignField.text = "";
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
            if (EventSystem.current.currentSelectedGameObject == shipNameField.gameObject)
            {
                shipCallsignField.Select();
            }
            else if (EventSystem.current.currentSelectedGameObject == shipCallsignField.gameObject)
            {
                confirmButton.Select();
            }
            else if (EventSystem.current.currentSelectedGameObject == confirmButton.gameObject)
            {
                shipNameField.Select();
            }
        }
    }
}
