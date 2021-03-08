using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    // Propriétés du message qui peuvent être assignés lors de la création d'un message
    public string messageText;
    public Ship linkedShip;
    public Planet linkedPlanet;

    /* CONSTRUCTEURS de messages :
     * - Permet de "construire" un objet de Type Message, dans le code seulement (pas lié à un objet unity)
     * - Pour construire un nouveau message, simplement déclarer une variable de type Message et utiliser new :
     * --- Message mySuperMessage = new Message(.....)
     * - Les paramètres entre parenthèses sont au choix, et serviront à formatter les messages selon ce qui est envoyé : 
     */

    // Constructeur pour TEXTE uniquement
    public Message(string messageText)
    {
        this.messageText = messageText;
    }

    // Constructeur pour TEXTE et SHIP associé (en but de créer un format de message UNIQUE provenant des vaisseaux)
    public Message(string messageText, Ship linkedShip)
    {
        this.messageText = messageText;
        this.linkedShip = linkedShip;
    }

    // Constructeur pour TEXTE et PLANET associée (en but de créer un format de message UNIQUE provenant des planètes)
    public Message(string messageText, Planet linkedPlanet)
    {
        this.messageText = messageText;
        this.linkedPlanet = linkedPlanet;
    }

    // Constructeur pour TEXTE, SHIP et PLANET (à voir si utile ou non)
    public Message(string messageText, Ship linkedShip, Planet linkedPlanet)
    {
        this.messageText = messageText;
        this.linkedShip = linkedShip;
        this.linkedPlanet = linkedPlanet;
    }
}
