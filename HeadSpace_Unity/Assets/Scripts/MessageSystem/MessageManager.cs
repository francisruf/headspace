using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageManager : MonoBehaviour
{
    // Singleton
    public static MessageManager instance;

    // File d'attente des messages
    private Queue<string> _messageQueue = new Queue<string>();

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
    }

    // Assigner les éléments de UI initiaux
    private void Start()
    {
        UpdateMessageCount();

        if (currentMessageText != null)
            currentMessageText.enabled = false;
    }

    // Fonction qui ajoute un message à la file d'attente (sans l'imprimer)
    public void QueueMessage(string newMessage)
    {
        _messageQueue.Enqueue(newMessage);
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

        if (currentMessageText != null)
        {
            currentMessageText.text = messageToPrint;
            currentMessageText.enabled = true;
        }

        UpdateMessageCount();
    }

    // Fonction qui met à jour l'indicateur de message, à partir du nombre de messages restants dans la Queue
    private void UpdateMessageCount()
    {
        if (messageCountText == null)
            return;

        // Clamp le nombre de messages à deux
        messageCountText.text = Mathf.Clamp(_messageQueue.Count, 0, 99).ToString("00");
    }

    //public void NewObjectDetectedNotif(Ship ship, GridStaticObject obj)
    //{
    //    string newMessageText = ship.shipName + " detected a " + obj.objectNameLine + " at " + obj.GridCoordinates;

    //    QueueMessage(newMessageText);

    //    Debug.Log(obj);
    //}

    public void NewPlanetDetectedNotif(Ship ship, Planet planet) {

        string newMessageText = ship.shipName + " discovered a new Planet at " + planet.GridCoordinates;

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
        //Message when ships collides with Cloud here
    }

    public void ContactWithWormholeNotif(Ship ship) {
        //Message when ships collides with Wormhole here
    }

    public void OutOfWormholeNotif(Ship ship) {
        //Message when ships exits a Wormhole
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

    public void MoveFinishedNotif(Ship ship) {

        string newMessageText = ship.shipName + " has completed his move at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    public void MoveAbortedNotif(Ship ship) {

        string newMessageText = ship.shipName + " aborted its MOVE command and is now at " + ship.currentPositionInGridCoords;

        QueueMessage(newMessageText);
    }

    //public void NewPlanetDetectedNotif(Ship ship, Planet planet)
    //{
    //    string newMessageText = ship.shipName + " detected a " + planet.objectNameLine + " at " + planet.GridCoordinates;

    //    QueueMessage(newMessageText);
    //}
}
