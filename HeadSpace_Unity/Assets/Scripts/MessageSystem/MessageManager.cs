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

    public void NewObjectNotification(Ship ship, GridStaticObject obj)
    {
        string newMessageText = ship.shipName + " has met " + obj.objectNameLine + " at " + obj.GridCoordinates;

        QueueMessage(newMessageText);
    }

    public void NewAnomalyNotification(Ship ship, GridTile_Anomaly anomaly)
    {
        string newMessageText = ship.shipName + " is in danger " + anomaly.tileType;

        QueueMessage(newMessageText);
    }

    public void NewPlanetNotification(Planet planet)
    {
        string newMessageText = planet.objectNameLine;

        QueueMessage(newMessageText);
    }
}
