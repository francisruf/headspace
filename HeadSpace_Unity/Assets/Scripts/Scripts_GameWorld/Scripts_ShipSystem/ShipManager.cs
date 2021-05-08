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
    public List<Ship> AllShips { get { return shipInventory; } }

    private int _activeShipsCount;
    public int ActiveShipsCount { get { return _activeShipsCount; } }

    // État de debug (visibilité des vaisseaux)
    private bool shipDebugVisible = true;

    [HideInInspector] public float shipHeatDistance;
    public LayerMask tileLayerMask;

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
        Ship.shipRemoved += OnShipDestroyed;
        Ship.shipStateChange += OnShipStateChange;
        LevelLoader.levelLoaded += OnLevelLoaded;
    }

    // Unsubscription
    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipUnavailable -= OnShipUnavailable;
        Ship.shipRemoved -= OnShipDestroyed;
        Ship.shipStateChange -= OnShipStateChange;
        LevelLoader.levelLoaded -= OnLevelLoaded;
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
        _activeShipsCount++;

        string shipCallsign;
        string shipName = GetDefaultName(ship, out shipCallsign);
        ship.InitializeShip(shipName, shipCallsign, ship.shipStartingState);
        UpdateShipInventoryUI();
        PlaceSingleShipOnMap(ship);
        ship.SpawnMarker();
    }

    // Enlever un ship de l'inventaire
    private void OnShipDestroyed(Ship ship)
    {
        _activeShipsCount--;
        UpdateShipInventoryUI();
    }

    // Enlever un ship de l'inventaire
    private void OnShipUnavailable(Ship ship)
    {
        _activeShipsCount--;
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
    public bool FindShipByCallsign(string shipCallsignToFind, out Ship foundShip)
    {
        string shipCallsignLowerCase = shipCallsignToFind.ToLower();   // Mettre le nom recherché en minuscules

        // Assigner variables de départ
        bool shipCallsignFound = false;
        foundShip = null;

        // Itérer dans shipInventory, et vérifier si un ship a le nom correspondant
        foreach (var ship in shipInventory)
        {
            // Si un ship correspondant trouvé, retourner VRAI et assigner le foundShip en paramètre de sortie
            if (ship.shipCallsign.ToLower() == shipCallsignLowerCase)
            {
                if (ship.CurrentShipState != ShipState.Disabled)
                {
                    foundShip = ship;
                    shipCallsignFound = true;
                    break;
                }
            }
        }

        if (!shipCallsignFound)
        {
            string errorMessage = "Syntax error : \n\n";
            errorMessage += "Invalid ship CODE.";

            if (MessageManager.instance != null)
                MessageManager.instance.GenericMessage(errorMessage, true);
        }

        // Sinon, retourner FAUX (le foundShip sera null)
        return shipCallsignFound;
    }

    // Fonction qui affiche ou non les sprites de tous les vaisseaux
    public void ToggleShipDebug(bool toggleON)
    {
        foreach (var ship in shipInventory)
        {
            ship.ToggleSprite(toggleON);
        }
    }

    private void PlaceSingleShipOnMap(Ship ship)
    {
        List<GridTile> allCandidateTiles = new List<GridTile>();
        List<GridTile> finalCandidates = new List<GridTile>();
        int minShipHeat = int.MaxValue;
        foreach (var tile in GridCoords.CurrentGridInfo.gameGridTiles)
        {
            if (tile.tileType == 0)
            {
                allCandidateTiles.Add(tile);
                if (tile.ShipStartHeat < minShipHeat)
                {
                    minShipHeat = tile.ShipStartHeat;
                }
            }
        }

        foreach (var sh in shipInventory)
        {
            if (sh == ship)
                continue;

            TileCoordinates shipPos = GridCoords.FromWorldToTilePosition(sh.transform.position);
            if (!GridCoords.IsInGrid(shipPos.tileX, shipPos.tileY))
                continue;

            GridTile tileToRemove = GridCoords.CurrentGridInfo.gameGridTiles[shipPos.tileX, shipPos.tileY];
            allCandidateTiles.Remove(tileToRemove);
        }

        int count = 0;
        foreach (var tile in allCandidateTiles)
        {
            if (tile.ShipStartHeat <= minShipHeat + 2)
            {
                finalCandidates.Add(tile);
                count++;
            }
        }
        int randomIndex = UnityEngine.Random.Range(0, count);
        Vector2 shipPosition = finalCandidates[randomIndex].TileCenter;
        TileCoordinates shipCoords = finalCandidates[randomIndex].TileCoordinates;
        ship.transform.position = shipPosition;

        // Add heat, one circle cast per heat distance
        for (int j = 1; j <= shipHeatDistance; j++)
        {
            float heatCastRadius = GridCoords.CurrentGridInfo.TileWidth * j;

            Collider2D[] allHits = Physics2D.OverlapCircleAll(shipPosition, heatCastRadius, tileLayerMask);
            foreach (var hit in allHits)
            {
                GridTile candidate = hit.GetComponent<GridTile>();
                if (hit != null)
                {
                    candidate.AddShipHeat(1);
                }
            }
        }

        foreach (var tile in allCandidateTiles)
        {
            // If same X
            if (tile.tileX == shipCoords.tileX)
                // If not exact same tile
                if (tile.tileY != shipCoords.tileY)
                {
                    // If neighbour
                    if (tile.tileY == shipCoords.tileY - 1 || tile.tileY == shipCoords.tileY + 1)
                        tile.AddShipHeat(2);
                    // If other
                    else
                        tile.AddShipHeat(2);
                }

            if (tile.tileY == shipCoords.tileY)
                // If not exact same tile
                if (tile.tileX != shipCoords.tileX)
                {
                    // If neighbour
                    if (tile.tileX == shipCoords.tileX - 1 || tile.tileX == shipCoords.tileX + 1)
                        tile.AddShipHeat(2);                    
                    // If other
                    else
                        tile.AddShipHeat(2);
                }
        }
    }
    private void OnLevelLoaded(int levelID)
    {
        if (levelID == 0)
            return;

        StartCoroutine(ShipStartNotif());
    }

    private IEnumerator ShipStartNotif()
    {
        yield return new WaitForSeconds(1f);
        MessageManager.instance.ShipStateMessage(shipInventory);
    }
}
