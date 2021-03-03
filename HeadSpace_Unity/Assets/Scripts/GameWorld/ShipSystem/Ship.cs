/* À faire, entre autres : 
 * - Déclarer une fonction publique par commande, et la lier aux fonctions ExecuteCommand des différents scripts de commandes
 * - "Enregistrer" chaque nouveau vaisseau au ShipManager (une action serait sick)
 * - Définir un ÉTAT de vaisseau (disponible / déployé / etc.), possiblement avec un ENUM
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    //STATS
    public string shipName;
    [Header("Stats")]

    [Range(0,100)]
    public int healthPoints;

    [Range(0, 100)]
    //How many souls can the Ship carry
    public int cargoCapacity;

    [Range(0, 10)]
    public float movementSpeed;

    [Range(0, 10)]
    //How many Souls per second can the Ship transfer from the Planet to Cargo
    public float pickupSpeed;

    [Range(0, 1)]
    public float detectionRadius;
    private CircleCollider2D detectionZone;

    //MOVEMENT
    public Vector2 testGridCoords;

    void Start()
    {
        //Sets the radius of the CircleCollider2D located in child GameObject<Detection_Collider> to be equal to the one set by the detectionRadius variable.
        detectionZone = GetComponentInChildren<CircleCollider2D>();
        detectionZone.radius = detectionRadius;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M)) {
        //    Move(testGridCoords);
        //}
    }

    //public void Move(Vector2 gridCoords) {

    //}

    //STEP 1: MOVE THAT SHIP
    //Créer une fonction MOVE qui prend un Vecto2 à l'entrée (GridCoords)
    //Coder le mouvement pour que ça marche independemment ici
    //Créer un champs public Vector2 qui prend des GridCoords
    //Convertir les GridCoords en WorldCoords (le faire DANS l'input)
    //Quand j'INPUT qqc, vaisseau bouge mouvement speed vers ces coordonnées

    //STEP 2: 
    //
}
