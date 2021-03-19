using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Classe à continuer.
 * J'ai seulement commencé pour pouvoir implémenter les bonnes références dans le CommandManager
 * 
 * Le ShipManager sert à conserver une référence à tous les Ship actifs, afin de dispatcher l'information au besoin :
 * -- Pour le UI d'inventaire
 * -- Pour la gestion des commandes
 * 
 * Pistes : 
 * - Utiliser une Liste qui se remplit à chaque vaisseau qui s'active (un inventaire, dans l'fond)
 * - Utiliser une fonction de recherche de vaisseau dans cette liste, à l'aide du NOM d'un vaisseau (un string public)
 */

public class ShipManager : MonoBehaviour
{
    // Action qui lance les updates de UI
    public static Action<List<Ship>> shipInventoryUpdate;

    // Singleton
    public static ShipManager instance;

    // Liste de tous les ships qui ont été déclarés
    private List<Ship> shipInventory = new List<Ship>();

    // État de debug (visibilité des vaisseaux)
    private bool shipDebugVisible = true;

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    // Subscription aux actions de Ship (ajouter ou enlever un Ship de l'inventaire)
    private void OnEnable()
    {
        Ship.newShipAvailable += OnNewShipAvailable;
        Ship.shipUnavailable += OnShipUnavailable;
        Ship.shipDestroyed += OnShipDestroyed;
        Ship.shipStateChange += OnShipStateChange;
    }

    // Unsubscription
    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipUnavailable -= OnShipUnavailable;
        Ship.shipDestroyed -= OnShipDestroyed;
        Ship.shipStateChange -= OnShipStateChange;
    }

    public string GetDefaultName(Ship newShip, out string defaultCallsign)
    {
        int candidateNumber = 1;
        string candidateName = newShip.shipClass + "00";
        bool valid = false;

        while (!valid)
        {
            bool sameName = false;

            candidateName = candidateName.Substring(0, candidateName.Length - 2);
            candidateName += candidateNumber.ToString("00");

            for (int i = 0; i < shipInventory.Count; i++)
            {
                if (candidateName == shipInventory[i].shipName)
                {
                    sameName = true;
                    break;
                }
            }
            if (sameName)
            {
                candidateNumber++;
                if (candidateNumber >= 100)
                {
                    break;
                }
            }
            else
            {
                valid = true;
            }
        }

        if (!valid)
        {
            defaultCallsign = "iii";
            return "iiiiiii";
        }
        defaultCallsign = GetDefaultCallsign(newShip, candidateNumber);
        return candidateName;
    }

    public string GetDefaultCallsign(Ship newShip, int defaultNumber)
    {
        string number = defaultNumber.ToString("00");

        string candidateCallsign = newShip.shipClass[0] + number;

        bool valid = true;
        for (int i = 0; i < shipInventory.Count; i++)
        {
            if (shipInventory[i].shipCallsign == candidateCallsign)
            {
                valid = false;
                break;
            }
        }

        if (!valid)
        {
            valid = true;
            candidateCallsign = "X" + number;
            for (int i = 0; i < shipInventory.Count; i++)
            {
                if (shipInventory[i].shipCallsign == candidateCallsign)
                {
                    valid = false;
                    break;
                }
            }
        }

        if (!valid)
        {
            while (!valid)
            {
                char currentChar = 'A';
                candidateCallsign = currentChar + number;

                valid = true;
                for (int i = 0; i < shipInventory.Count; i++)
                {
                    if (shipInventory[i].shipCallsign == candidateCallsign)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                {
                    currentChar++;
                    if (currentChar > 'Z')
                    {
                        break;
                    }
                }
            }
        }

        if (!valid)
        {
            return "$$$";
        }
        return candidateCallsign;
    }

    // Ajouter un ship à l'inventaire
    private void OnNewShipAvailable(Ship ship)
    {
        shipInventory.Add(ship);

        string shipCallsign;
        string shipName = GetDefaultName(ship, out shipCallsign);
        ship.InitializeShip(shipName, shipCallsign, ship.shipStartingState);
        UpdateShipInventoryUI();
    }

    // Enlever un ship de l'inventaire
    private void OnShipDestroyed(Ship ship)
    {
        UpdateShipInventoryUI();
    }

    // Enlever un ship de l'inventaire
    private void OnShipUnavailable(Ship ship)
    {
        if (shipInventory.Contains(ship))
        {
            shipInventory.Remove(ship);
            UpdateShipInventoryUI();
        }
    }

    // Fonction qui se trigger lorsqu'un vaisseau change d'état
    // Pour l'instant, ne sert qu'à notifier le ShipUI
    private void OnShipStateChange(Ship ship)
    {
        UpdateShipInventoryUI();
    }

    // Fonction générale pour trigger le Update du UI
    private void UpdateShipInventoryUI()
    {
        if (shipInventoryUpdate != null)
            shipInventoryUpdate(shipInventory);
    }

    /* Fonction qui retourne VRAI ou FAUX si un vaisseau a été trouvé.
     * La fonction assigne également un paramètre de SORTIE avec la clause OUT, permettant d'envoyer
     * une référence au vaisseau trouvé du même coup.
     */
    public bool FindShipByName(string shipNameToFind, out Ship foundShip)
    {
        string shipNameLowerCase = shipNameToFind.ToLower();   // Mettre le nom recherché en minuscules

        // Assigner variables de départ
        bool shipNameFound = false;
        foundShip = null;

        // Itérer dans shipInventory, et vérifier si un ship a le nom correspondant
        foreach (var ship in shipInventory)
        {
            // Si un ship correspondant trouvé, retourner VRAI et assigner le foundShip en paramètre de sortie
            if (ship.shipName.ToLower() == shipNameLowerCase)
            {
                if (ship.CurrentShipState != ShipState.Destroyed)
                {
                    foundShip = ship;
                    shipNameFound = true;
                    break;
                }
            }
        }
        // Sinon, retourner FAUX (le foundShip sera null)
        return shipNameFound;
    }

    // Fonction qui affiche ou non les sprites de tous les vaisseaux
    public void ToggleShipDebug()
    {
        // Si debug visible au moment du toggle, désactiver les sprites des vaisseaux dont l'état est DEPLOYED
        if (shipDebugVisible)
        {
            foreach (var ship in shipInventory)
            {
                if (ship.CurrentShipState == ShipState.Deployed)
                    ship.ToggleSprite(false);
            }
            shipDebugVisible = false;
        }
        // Si debug non visible au moment du toggle, activer les sprites des vaisseaux dont l'état est DEPLOYED
        else
        {
            foreach (var ship in shipInventory)
            {
                if (ship.CurrentShipState == ShipState.Deployed)
                    ship.ToggleSprite(true);
            }
            shipDebugVisible = true;
        }
    }
}
