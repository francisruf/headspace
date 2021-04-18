using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public static Action newMessageReceived;

    // Singleton
    public static MessageManager instance;

    // Référence au récepteur
    private Receiver _receiver;

    // File d'attente des messages
    private Queue<string> _messageQueue = new Queue<string>();
    private int _messageCount;

    // Textes de DEBUG temporaires
    public TextMeshProUGUI currentMessageText;
    public TextMeshProUGUI messageCountText;

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

        _receiver = GetComponentInParent<Receiver>();
    }

    // Assigner les éléments de UI initiaux
    private void Start()
    {
        UpdateMessageCount();

        if (currentMessageText != null)
            currentMessageText.enabled = false;
    }

    private void LateUpdate()
    {
        if (_messageCount > 0)
        {
            if (_receiver.CanPrintMessages())
            {
                PrintNextMessage();
            }
        }
    }

    // Fonction qui ajoute un message à la file d'attente (sans l'imprimer)
    public void QueueMessage(string newMessage)
    {
        string fullMessage = "";
        if (TimeManager.instance != null)
        {
            fullMessage += "[" + TimeSpan.FromSeconds(TimeManager.instance.GameTime).ToString(@"hh\:mm") + "].............\n\n";
        }
        fullMessage += newMessage;

        _messageQueue.Enqueue(fullMessage);
        _messageCount++;

        if (newMessageReceived != null)
            newMessageReceived();

        UpdateMessageCount();
    }

    // Fonction qui imprime le prochain message de la file d'attente
    public void PrintNextMessage()
    {
        if (_messageQueue.Count <= 0)
        {
            if (currentMessageText != null)
                currentMessageText.enabled = false;

            return;
        }

        string messageToPrint = _messageQueue.Dequeue();
        _messageCount--;

        if (currentMessageText != null)
        {
            currentMessageText.text = messageToPrint;
            currentMessageText.enabled = true;
        }

        _receiver.PrintMessage(messageToPrint);
        UpdateMessageCount();
    }

    // Fonction qui met à jour l'indicateur de message, à partir du nombre de messages restants dans la Queue
    private void UpdateMessageCount()
    {
        if (messageCountText == null)
            return;

        // Clamp le nombre de messages à deux
        messageCountText.text = Mathf.Clamp(_messageCount, 0, 99).ToString("00");
    }

    //NOTIFICATION FUNCTIONS
    #region
    public void NewPlanetDetectedNotif(Ship ship, Planet planet) {

        string newMessageText = ship.shipName + " discovered a new Planet at " + planet.GridCoordinates;

        QueueMessage(newMessageText);
    }

    public void NewHazardDetectedNotif(Ship ship, Hazard hazard) {

        string newMessageText = ship.shipName + " discovered a new " + hazard.objectNameLine + " at " + hazard.GridCoordinates;

        QueueMessage(newMessageText);
    }

    public void NewTransmissionDetectedNotif(Ship ship, Planet planet) {

        string newMessageText = ship.shipName + " discovered a new Transmission at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void NewAnomalyDetectedNotif(Ship ship, GridTile_Anomaly anomaly)
    {
        string newMessageText = ship.shipName + " reports that tile " + anomaly.tileX + ", " + anomaly.tileY + " has been affected";

        QueueMessage(newMessageText);
    }

    public void ContactWithAnomalyNotif(Ship ship, GridTile_Anomaly anomaly) 
    {
        string newMessageText = "DANGER: " + ship.shipName + " currently affected by Stage " + anomaly.tileType + " Anomaly at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void ContactWithCloudNotif(Ship ship) {

        string newMessageText = ship.shipName + " $t#ck &ns&d3 m%gn3ti( $l#ud &t " + ship.currentPositionInGridCoords + " Pl3@$e s&nd #ver n%w c#mm@nd.";

        QueueMessage(newMessageText);
    }

    public void ContactWithWormholeNotif(Ship ship) {

        string newMessageText = ship.shipName + " entered a Wormhole at " + ship.currentPositionInGridCoords + ". New MOVE target set to " + ship.targetWorldCoords;

        QueueMessage(newMessageText);
    }

    public void EnteredPlanetOrbitNotif(Ship ship, Planet planet) {

        if (planet.TotalSouls <= 0) {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . This planet is uninhabited.";

            QueueMessage(newMessageText);
        }

        else if (planet.CurrentSouls <= 0) {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . No more souls on this Planet.";

            QueueMessage(newMessageText);
        }

        else {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . There are " + planet.CurrentSouls + " souls on this Planet. Ready to LOAD.";

            QueueMessage(newMessageText);
        }

    }

    public void EnteredDeployPointNotif(Ship ship, DeployPoint deploy) {

        string newMessageText = ship.shipName + " entered the Deploy Point at " + deploy.GridCoordinates + " and is now ready to leave the system.";

        QueueMessage(newMessageText);
    }

    public void ShipDeployedNotif(Ship ship) {

        string newMessageText = ship.shipName + " is now Deployed at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void MoveFinishedNotif(Ship ship) {

        string newMessageText = ship.shipName + " has completed his move at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void MoveAbortedNotif(Ship ship) {

        string newMessageText = ship.shipName + " aborted its MOVE command and is now at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void PickupAbortedNotif(Ship ship) {

        string newMessageText = ship.shipName + " aborted its PICKUP command at " + ship.currentPositionInGridCoords + ". Cargo status: " + ship.currentCargo + " / " + ship.cargoCapacity;

        QueueMessage(newMessageText);
    }

    public void InvalidDestinationNotif(Ship ship, string message)
    {
        string newMessageText = ship.shipName + " reports nav problem";
        if (message != "")
            newMessageText += " : " + message;
        else
            newMessageText += ".";

        QueueMessage(newMessageText);
    }

    public void PickupFinishedNotif(Ship ship) {

        string newMessageText = ship.shipName + " finished its PICKUP command at " + ship.currentPositionInGridCoords + ". Cargo status: " + ship.currentCargo + " / " + ship.cargoCapacity;

        QueueMessage(newMessageText);
    }

    public void RouteFinishedNotif(Ship ship)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);

        string newMessageText = ship.shipName + " has completed his route at " + tileName + ".";

        QueueMessage(newMessageText);
    }

    public void BonusCreditsNotif(Planet planet)
    {
        string creditText = planet.CompletionCreditsBonus > 1 ? "credits" : "credit";

        string newMessageText = "Relayed message :";
        newMessageText += "\nThank you for saving all of our people. Please take this as a token of our gratitude";
        newMessageText += "\n\n[" + planet.CompletionCreditsBonus + " additional " + creditText + " awarded.]";

        QueueMessage(newMessageText);
    }

    public void ShipStatusNotif(Ship ship)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);

        string newMessageText = "Ship status request : ";
        newMessageText += ship.shipName + " at position " + tileName;
        newMessageText += "\nMental health : " + ship.currentHealthPoints;
        newMessageText += "\nClients on board : ";

        int clientCount = ship.ClientsOnBoard.Count;
        if (clientCount > 0)
        {
            for (int i = 0; i < clientCount; i++)
            {
                newMessageText += ship.ClientsOnBoard[i].clientFirstName[0] + ". " + ship.ClientsOnBoard[i].clientLastName[0];

                if (i == clientCount - 1)
                    newMessageText += ".";
                else
                    newMessageText += ", ";
            }
        }
        else
        {
            newMessageText += "none.";
        }
        QueueMessage(newMessageText);
    }

    public void ShipAnomalyNotif(Ship ship, int soulsLost)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);
        string newMessageText = ship.shipName + " has been attacked by an anomaly. ";

        if (soulsLost > 1)
            newMessageText += soulsLost + " souls on board have been lost to folley. ";
        else if (soulsLost == 1)
            newMessageText += soulsLost + " soul on board has been lost to folley. ";

        newMessageText += "Please wait for auto-reinitialization.";
        QueueMessage(newMessageText);
    }

    #endregion
}
