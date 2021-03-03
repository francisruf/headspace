/* À faire, entre autres : 
 * - Déclarer une fonction publique par commande, et la lier aux fonctions ExecuteCommand des différents scripts de commandes
 * - "Enregistrer" chaque nouveau vaisseau au ShipManager (une action serait sick)
 * - Définir un ÉTAT de vaisseau (disponible / déployé / etc.), possiblement avec un ENUM
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    // Actions that send various updates regarding their state/availability
    public static Action<Ship> newShipAvailable;
    public static Action<Ship> shipUnavailable;
    public static Action<Ship> shipStateChange;

    // Components
    private SpriteRenderer spriteRenderer;

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

    // STATE TRACKING
    // Ça c'est une "propriété", aka une autre façon fancy d'écrire des variables
    public ShipState CurrentShipState { get; private set; } = ShipState.Deployed;    // Pour l'instant, les Ships sont considérés Deployed

    private void Awake()
    {
        // Assign component references
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        //Fire the newShipAvaible action (received by the ShipManager)
        if (newShipAvailable != null)
            newShipAvailable(this);

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

    public void Move(Vector2 gridCoords)
    {
        Debug.Log("MOVE called on Ship : " + shipName);
    }

    //STEP 1: MOVE THAT SHIP
    //Créer une fonction MOVE qui prend un Vecto2 à l'entrée (GridCoords)
    //Coder le mouvement pour que ça marche independemment ici
    //Créer un champs public Vector2 qui prend des GridCoords
    //Convertir les GridCoords en WorldCoords (le faire DANS l'input)
    //Quand j'INPUT qqc, vaisseau bouge mouvement speed vers ces coordonnées

    //STEP 2: 
    //

    // Function that changes and tracks the ship state (AtBase / Deployed) and notifies other scripts
    private void ChangeShipState(ShipState newState)
    {
        CurrentShipState = newState;

        // Action call when a ship changes state
        if (shipStateChange != null)
            shipStateChange(this);
    }

    // Simple function that toggles SpriteRender on/off, called from DebugManager-->ShipManager-->Ship
    public void ToggleSprite(bool toggleON)
    {
        spriteRenderer.enabled = toggleON;
    }

    // Safety net - When a ship is disabled, sends info to the ShipManager (TODO : Nothing calls DisableShip yet)
    // Function to be implemented when a ship is fully destroyed, or when the scene changes
    private void DisableShip()
    {
        if (shipUnavailable != null)
            shipUnavailable(this);
    }
}

public enum ShipState
{
    Deployed,
    AtBase
}