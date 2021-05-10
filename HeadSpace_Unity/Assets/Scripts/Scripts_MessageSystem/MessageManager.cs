using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageManager : MonoBehaviour
{
    public static Action<bool> newMessageReceived;

    [Header("Message colors")]
    public Color defaultColor;
    public Color shipMessageColor;
    public Color timeMessageColor;
    public Color contractMessageColor;
    public Color dangerMessageColor;

    // Singleton
    public static MessageManager instance;

    // Référence au récepteur
    private Receiver _receiver;

    // File d'attente des messages
    private Queue<MessageInfo> _messageQueue = new Queue<MessageInfo>();
    private int _messageCount;

    // Textes de DEBUG temporaires
    public TextMeshProUGUI currentMessageText;
    public TextMeshProUGUI messageCountText;

    private void OnEnable()
    {
        TimeManager.halfWarning += HalfTimeWarning;
        TimeManager.threeQuarterWarning += ThreeQuarterTimeWarning;
    }

    private void OnDisable()
    {
        TimeManager.halfWarning -= HalfTimeWarning;
        TimeManager.threeQuarterWarning -= ThreeQuarterTimeWarning;
    }

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
    public void QueueMessage(string newMessage, Color messageColor, bool playSound = true)
    {
        MessageInfo newMessageInfo = new MessageInfo("", newMessage, messageColor, false);
        StartCoroutine(QueueDelay(newMessageInfo, playSound));
    }

    // Fonction qui ajoute un message à la file d'attente (sans l'imprimer)
    public void QueueMessage(MessageInfo newMessageInfo, bool playSound = true)
    {
        StartCoroutine(QueueDelay(newMessageInfo, playSound));
    }

    private IEnumerator QueueDelay(MessageInfo newMessageInfo, bool playSound)
    {
        yield return new WaitForSeconds(1f);
        _messageQueue.Enqueue(newMessageInfo);
        _messageCount++;

        UpdateMessageCount();

        if (newMessageReceived != null)
            newMessageReceived(playSound);
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

        MessageInfo messageInfo = _messageQueue.Dequeue();
        _messageCount--;

        if (messageInfo.specialMessage)
            _receiver.PrintTutorialMessage(messageInfo.messageName, messageInfo.messageText, messageInfo.messageColor);
        else
            _receiver.PrintMessage(messageInfo.messageText, messageInfo.messageColor);
        
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

        QueueMessage(newMessageText, defaultColor);
    }

    public void NewHazardDetectedNotif(Ship ship, Hazard hazard) {

        string newMessageText = ship.shipName + " discovered a new " + hazard.objectNameLine + " at " + hazard.GridCoordinates;

        QueueMessage(newMessageText, defaultColor);
    }

    public void NewTransmissionDetectedNotif(Ship ship, Planet planet) {

        string newMessageText = ship.shipName + " discovered a new Transmission at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void NewAnomalyDetectedNotif(Ship ship, GridTile_Anomaly anomaly)
    {
        string newMessageText = ship.shipName + " reports that tile " + anomaly.tileX + ", " + anomaly.tileY + " has been affected";

        QueueMessage(newMessageText, defaultColor);
    }

    public void ContactWithAnomalyNotif(Ship ship, GridTile_Anomaly anomaly) 
    {
        string newMessageText = "DANGER: " + ship.shipName + " currently affected by Stage " + anomaly.tileType + " Anomaly at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void ContactWithCloudNotif(Ship ship) {

        string newMessageText = ship.shipName + " $t#ck &ns&d3 m%gn3ti( $l#ud &t " + ship.currentPositionInGridCoords + " Pl3@$e s&nd #ver n%w c#mm@nd.";

        QueueMessage(newMessageText, defaultColor);
    }

    public void ContactWithWormholeNotif(Ship ship) {

        string newMessageText = ship.shipName + " entered a Wormhole at " + ship.currentPositionInGridCoords + ". New MOVE target set to " + ship.targetWorldCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void EnteredPlanetOrbitNotif(Ship ship, Planet planet) {

        if (planet.TotalSouls <= 0) {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . This planet is uninhabited.";

            QueueMessage(newMessageText, defaultColor);
        }

        else if (planet.CurrentSouls <= 0) {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . No more souls on this Planet.";

            QueueMessage(newMessageText, defaultColor);
        }

        else {
            string newMessageText = ship.shipName + " is now in orbit of Planet at " + planet.GridCoordinates + " . There are " + planet.CurrentSouls + " souls on this Planet. Ready to LOAD.";

            QueueMessage(newMessageText, defaultColor);
        }

    }

    public void EnteredDeployPointNotif(Ship ship, DeployPoint deploy) {

        string newMessageText = ship.shipName + " entered the Deploy Point at " + deploy.GridCoordinates + " and is now ready to leave the system.";

        QueueMessage(newMessageText, defaultColor);
    }

    public void ShipDeployedNotif(Ship ship) {

        string newMessageText = ship.shipName + " is now Deployed at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void MoveFinishedNotif(Ship ship) {

        string newMessageText = ship.shipName + " has completed his move at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void MoveAbortedNotif(Ship ship) {

        string newMessageText = ship.shipName + " aborted its MOVE command and is now at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText, defaultColor);
    }

    public void PickupAbortedNotif(Ship ship) {

        string newMessageText = ship.shipName + " aborted its PICKUP command at " + ship.currentPositionInGridCoords + ". Cargo status: " + ship.currentCargo + " / " + ship.cargoCapacity;

        QueueMessage(newMessageText, defaultColor);
    }

    public void PickupFinishedNotif(Ship ship)
    {

        string newMessageText = ship.shipName + " finished its PICKUP command at " + ship.currentPositionInGridCoords + ". Cargo status: " + ship.currentCargo + " / " + ship.cargoCapacity;

        QueueMessage(newMessageText, shipMessageColor);
    }

    public void BonusCreditsNotif(Planet planet)
    {
        string creditText = planet.CompletionCreditsBonus > 1 ? "credits" : "credit";

        string newMessageText = "Relayed message :";
        newMessageText += "\nThank you for saving all of our people. Please take this as a token of our gratitude";
        newMessageText += "\n\n[" + planet.CompletionCreditsBonus + " additional " + creditText + " awarded.]";

        QueueMessage(newMessageText, shipMessageColor);
    }

    public void InvalidDestinationNotif(Ship ship, string message)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);

        string newMessageText = ship.shipName + " reports ROUTE problem";
        if (message != "")
            newMessageText += ":\n" + message;
        else
            newMessageText += ".";

        newMessageText += " Current position: " + tileName + "\nAwaiting instructions.";

        QueueMessage(newMessageText, shipMessageColor);
    }

    public void RouteFinishedNotif(Ship ship)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);

        string newMessageText = ship.shipName + " has completed its ROUTE at " + tileName + ".";

        QueueMessage(newMessageText, shipMessageColor, false);
    }

    public void ShipStopMovementNotif(Ship ship, bool isMoving)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);
        string newMessageText = "";

        if (isMoving)
            newMessageText = ship.shipName + " has aborted its ROUTE at position " + tileName + ".";
        else
            newMessageText = "Command error :" + ship.shipName + " is already idle at position " + tileName + ".";

        newMessageText += "\n\nAwaiting instructions.";

        //int clientCount = ship.ClientsOnBoard.Count;
        //if (clientCount > 0)
        //{
        //    for (int i = 0; i < clientCount; i++)
        //    {
        //        newMessageText += ship.ClientsOnBoard[i].clientFirstName[0] + ". " + ship.ClientsOnBoard[i].clientLastName[0];

        //        if (i == clientCount - 1)
        //            newMessageText += ".";
        //        else
        //            newMessageText += ", ";
        //    }
        //}
        //else
        //{
        //    newMessageText += "none.";
        //}
        QueueMessage(newMessageText, shipMessageColor);
    }

    public void ShipStatusNotif(Ship ship)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);

        string newMessageText = "Ship status request : ";
        newMessageText += "\n" + ship.shipName + " (" + ship.shipCallsign + ") at position " + tileName + ".";
        newMessageText += "\n\nOfficials on board : ";

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
        QueueMessage(new MessageInfo("CMD_Status", newMessageText, shipMessageColor, true));
    }

    public void ShipAnomalyNotif(Ship ship, int soulsLost)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);
        string newMessageText = ship.shipName + " has been attacked by an anomaly.";

        if (soulsLost > 1)
        {
            newMessageText += "\n" + soulsLost + " souls on board have been lost to folley. ";
        }
        else if (soulsLost == 1)
        {
            newMessageText += "\n" + soulsLost + " soul on board has been lost to folley. ";
        }
        else
            newMessageText += "\n";

        newMessageText += "Please wait for auto-reinitialization.";

        QueueMessage(newMessageText, dangerMessageColor, false);
    }

    public void ShipResetNotif(Ship ship)
    {
        TileCoordinates shipTile = GridCoords.FromWorldToTilePosition(ship.transform.position);
        string tileName = GridCoords.GetTileName(shipTile);
        string newMessageText = ship.shipName + " auto-reinitialization complete.";
        newMessageText += "Current position : " + tileName + ".";
        newMessageText += "\nAwaiting instructions.";

        QueueMessage(newMessageText, shipMessageColor, false);
    }

    public void ClientEmbarkedNotif(Ship ship, Client client, GridTile_Planet planet)
    {
        string newMessageText = ship.shipName + " has picked up " + client.clientFirstName + " "
            + client.clientLastName + " on planet " + planet.PlanetName + ".";

        QueueMessage(newMessageText, contractMessageColor, true);
    }

    public void ClientDebarkedNotif(Ship ship, Client client, GridTile_Planet planet)
    {
        string newMessageText = ship.shipName + " has debarked " + client.clientFirstName + " "
            + client.clientLastName + " on planet " + planet.PlanetName + ".";

        newMessageText += "\nWORK ORDER COMPLETE!";

        QueueMessage(newMessageText, contractMessageColor, true);
    }

    public void ShipStateMessage(List<Ship> allShips)
    {
        string message = "FLEET STATUS : ";
        for (int i = 0; i < allShips.Count; i++)
        {
            string tileName = GridCoords.GetTileName(GridCoords.FromWorldToTilePosition(allShips[i].transform.position));
            message += "\n" + allShips[i].shipName + " position : " + tileName;
        }
        QueueMessage(message, shipMessageColor);
    }

    public void HalfTimeWarning()
    {
        string message = "[STM auto time tracker]\n\n4 HOURS REMAINING";
        QueueMessage(message, timeMessageColor);
    }

    public void ThreeQuarterTimeWarning()
    {
        string message = "[STM auto time tracker]\n\n2 HOURS REMAINING";
        QueueMessage(message, timeMessageColor);
    }

    public void GenericMessage(string message, bool playSound)
    {
        QueueMessage(message, defaultColor, playSound);
    }

    public void SpecialMessage(string messageName, string messageText)
    {
        QueueMessage(new MessageInfo(messageName, messageText, defaultColor, true));
    }

    #endregion
}

public struct MessageInfo
{
    public string messageName;
    public string messageText;
    public Color messageColor;

    public bool specialMessage;

    public MessageInfo(string messageName, string messageText, Color messageColor, bool specialMessage)
    {
        this.messageName = messageName;
        this.messageText = messageText;
        this.messageColor = messageColor;
        this.specialMessage = specialMessage;
    }
}
