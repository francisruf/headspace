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
    private PolygonCollider2D shipCollider;
    private CircleCollider2D detectionZone;

    //STATS
    public string shipName;
    [Header("Stats")]

    [Range(0,100)]
    public int healthPoints;

    [Range(0, 100)]
    //How many souls can the Ship carry
    public int cargoCapacity;

    [Range(0, 100)]
    public float moveSpeed;

    [Range(0, 10)]
    //How many Souls per second can the Ship transfer from the Planet to Cargo
    public float pickupSpeed;

    [Range(0, 1)]
    public float detectionRadius;

    //MOVEMENT
    private Vector2 displayedGridCoords;
    private Vector2 targetWorldCoords;
    private bool isMoving;

    //LEAVE
    private Vector2 basePosition;

    // STATE TRACKING
    public ShipState shipStartingState; // State du vaisseau lorsque Start() est appelé. Simplement pour debug et assigner un state différent.
    public ShipState CurrentShipState { get; private set; }  // Ça c'est une "propriété", aka une autre façon fancy d'écrire des variables

    private void Awake()
    {
        // Assign component references
        spriteRenderer = GetComponent<SpriteRenderer>();
        shipCollider = GetComponentInChildren<PolygonCollider2D>();
        detectionZone = GetComponentInChildren<CircleCollider2D>();
    }

    void Start()
    {
        // Assigner le currentState au state de départ
        CurrentShipState = shipStartingState;

        //Fire the newShipAvaible action (received by the ShipManager)
        if (newShipAvailable != null)
            newShipAvailable(this);

        //Sets the radius of the CircleCollider2D located in child GameObject<Detection_Collider> to be equal to the one set by the detectionRadius variable.
        detectionZone.radius = detectionRadius;

        //Assign basePosition
        basePosition = new Vector2(100f, 100f);

        //Start ships at basePosition
        transform.position = basePosition;
    }

    void Update()
    {
        //If MOVE command is called, moves the ship at coordinates indicated in the MOVE command.
        if (isMoving) {
            Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Move " + displayedGridCoords + " | STATUS: Moving");
            transform.position = Vector2.MoveTowards(transform.position, targetWorldCoords, moveSpeed * Time.deltaTime);
        }
        if (targetWorldCoords == (Vector2)transform.position) {
            isMoving = false;
        }
    }

    public void Deploy(Vector2 gridCoords) {

        //Stops function if Ship cannot be Deployed
        if (CurrentShipState == ShipState.Deployed) {
            Debug.Log("Ship already Deployed");
            return;
        }

        Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Deploy " + gridCoords + " | STATUS: Deployed");
        //Enable the ship and all it's components
        spriteRenderer.enabled = true;
        shipCollider.enabled = true;
        detectionZone.enabled = true;

        //Convert gridCoords entered into worldCoords
        targetWorldCoords = GridCoords.FromGridToWorld(gridCoords);
        //Place ship on the entered coordinates
        transform.position = targetWorldCoords;

        //Change the status of the ship from "At Base" to "Deployed"
        ChangeShipState(ShipState.Deployed);
    }

    public void Leave() {

        //Stops function if Ship cannot be Leave
        if (CurrentShipState == ShipState.AtBase) {
            Debug.Log("Ship is already At Base");
            return;
        }

        Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Leave | STATUS: On it's way to base");
        //Enable the ship and all it's components
        spriteRenderer.enabled = false;
        shipCollider.enabled = false;
        detectionZone.enabled = false;

        //Place ship on the entered coordinates
        transform.position = basePosition;

        //Change the status of the ship from "At Base" to "Deployed"
        ChangeShipState(ShipState.AtBase);
    }

    public void Move(Vector2 gridCoords) {

        //When MOVE command is called, it converts gridCoords to WorldCoords and sets isMoving to true
        displayedGridCoords = gridCoords;
        targetWorldCoords = GridCoords.FromGridToWorld(gridCoords);
        isMoving = true;
    }

    public void Abort() {
        Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Abort | STATUS: Action Stopped");
        isMoving = false;
    }

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