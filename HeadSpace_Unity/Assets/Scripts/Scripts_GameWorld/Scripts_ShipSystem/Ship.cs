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
    public static Action<Ship> shipDisabled;
    public static Action<Ship> shipDestroyed;
    public static Action<Ship> shipStateChange;
    public static Action<Ship> shipInfoChange;
    public static Action<int> soulsUnloaded;
    public static Action<PlanetSoulsMatch> soulsFromPlanetSaved;

    // Components
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D shipCollider;
    private CircleCollider2D detectionZone;

    // Linked Marker
    private ShipMarker linkedMarker;
    public GameObject markerPrefab;

    //STATS
    public string shipName;
    public string shipCallsign;
    [Header("Stats")]

    [Range(0, 100)]
    public int healthPoints;
    public int currentHealthPoints;

    [Range(0, 100)]
    //How many souls can the Ship carry
    public int cargoCapacity;
    [HideInInspector] public int currentCargo;

    [Range(0, 100)]
    public float moveSpeed;

    [Range(0, 10)]
    //How many seconds to pickup ONE Soul
    public float pickupSpeedInSeconds;

    [Range(0, 10)]
    public int soulsUnloadedPerSec;

    [Range(0, 1)]
    public float detectionRadius;


    //MOVEMENT
    private Vector2 displayedGridCoords;
    [HideInInspector] public Vector2 targetWorldCoords;
    [HideInInspector] public bool isMoving;

    //LEAVE
    private Vector2 basePosition;

    // STATE TRACKING
    public ShipState shipStartingState; // State du vaisseau lorsque Start() est appelé. Simplement pour debug et assigner un state différent.
    public ShipState CurrentShipState { get; private set; }  // Ça c'est une "propriété", aka une autre façon fancy d'écrire des variables

    // PLANET-SOULS TRACKING
    private List<PlanetSoulsMatch> _allSoulsAndPlanets = new List<PlanetSoulsMatch>();

    // FOR NOTIFICATIONS
    [HideInInspector] public Vector2 currentPositionInGridCoords;
    private MessageManager mM;
    [HideInInspector] public bool isInDeployPoint;
    [HideInInspector] public bool isInPlanetOrbit;
    [HideInInspector] public bool isLoadingSouls;
    [HideInInspector] public bool isInCloud;
    [HideInInspector] public bool isInWormHole;

    [HideInInspector] public Planet planetInOrbit;
    [HideInInspector] public DeployPoint deployP;
    [HideInInspector] public WormHole senderWormhole;
    [HideInInspector] public WormHole receiverWormhole;
    [HideInInspector] public GridTile_Anomaly anomalyTile;

    private IEnumerator pickupCoroutine;
    private IEnumerator damageTickCoroutine;

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
        currentHealthPoints = healthPoints;

        //Sets the radius of the CircleCollider2D located in child GameObject<Detection_Collider> to be equal to the one set by the detectionRadius variable.
        detectionZone.radius = detectionRadius;

        // Force captial letters for callsign and 3 chars. max (safety)
        shipCallsign = shipCallsign.ToUpper();
        if (shipCallsign.Length > 3)
        {
            shipCallsign = shipCallsign.Substring(0, 3);
        }

        //Assign basePosition
        //basePosition = new Vector2(100f, 100f);
        basePosition = new Vector2(0f, 0f); //ONLY FOR TESTING

        //Start ships at basePosition
        transform.position = basePosition;

        SpawnMarker();

        //Fire the newShipAvaible action (received by the ShipManager)
        if (newShipAvailable != null)
            newShipAvailable(this);

        mM = MessageManager.instance;
    }

    void Update()
    {
        CheckForAnomaly();
        MoveShip();

        //Finds current position at all times in Grid Coords
        currentPositionInGridCoords = GridCoords.FromWorldToGrid(transform.position);
    }

    private void MoveShip()
    {
        //If MOVE command is called, moves the ship at coordinates indicated in the MOVE command.
        if (isMoving)
        {
            //Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Move " + displayedGridCoords + " | STATUS: Moving");
            transform.position = Vector2.MoveTowards(transform.position, targetWorldCoords, moveSpeed * Time.deltaTime);

            if (targetWorldCoords == (Vector2)transform.position)
            {
                isMoving = false;
                //Debug.Log("Movement has ended");

                if (planetInOrbit != null)
                {
                    mM.EnteredPlanetOrbitNotif(this, planetInOrbit);
                }

                else if (deployP != null)
                {
                    mM.EnteredDeployPointNotif(this, deployP);
                }

                else
                {
                    mM.MoveFinishedNotif(this);
                }
            }
        }
    }

    // Function that initializes ship parameters when instantiated
    public void InitializeShip(string shipName, string shipCallsign, ShipState startingState)
    {
        shipStartingState = startingState;
        ChangeShipName(shipName, shipCallsign);
        ChangeShipState(startingState);
    }

    // Function that spawns a marker when a ship is activated (in start) and assigns its info.
    private void SpawnMarker()
    {
        if (markerPrefab != null)
        {
            // For now, spawn a marker at center.
            linkedMarker = Instantiate(markerPrefab).GetComponentInChildren<ShipMarker>();
            linkedMarker.InitializeMarker(this);
        }
    }

    public void Deploy(Vector2 gridCoords) {

        //Stops function if Ship cannot be Deployed
        if (CurrentShipState == ShipState.Deployed) {
            Debug.Log("Ship already Deployed");
            return;
        }

        //Stops function if Ship cannot be Deployed
        if (CurrentShipState == ShipState.Unloading)
        {
            Debug.Log("Ship is still unloading");
            return;
        }

        //Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Deploy " + gridCoords + " | STATUS: Deployed");

        //Convert gridCoords entered into worldCoords
        targetWorldCoords = GridCoords.FromGridToWorld(gridCoords);
        //Place ship on the entered coordinates
        transform.position = targetWorldCoords;

        //Change the status of the ship from "At Base" to "Deployed"
        ChangeShipState(ShipState.Deployed);

        //Notify player that Ship has Deployed
        mM.ShipDeployedNotif(this);
    }

    public void Leave() {

        //Stops function if Ship cannot be Leave
        if (CurrentShipState != ShipState.Deployed) {
            Debug.Log("Ship is already At Base");
            return;
        }

        if (isMoving)
            isMoving = false;

        //Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Leave | STATUS: On it's way to base");


        //Place ship on the entered coordinates
        transform.position = basePosition;

        ChangeShipState(ShipState.Unloading);
    }

    public void Move(Vector2 gridCoords) {

        if (CurrentShipState != ShipState.Deployed)
        {
            Debug.Log("Ship is not deployed");
            return;
        }

        if (pickupCoroutine != null) {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }

        //When MOVE command is called, it converts gridCoords to WorldCoords and sets isMoving to true
        displayedGridCoords = gridCoords;
        targetWorldCoords = GridCoords.FromGridToWorld(gridCoords);
        isMoving = true;
    }

    public void Pickup() {

        if (CurrentShipState != ShipState.Deployed) {
            Debug.Log("Ship is not deployed");
            return;
        }

        if (planetInOrbit != null) {

            if (pickupCoroutine == null)
                pickupCoroutine = LoadingSouls();
            StartCoroutine(pickupCoroutine);
        }
    }

    public void Abort() {

        if (isMoving) {
            isMoving = false;
            mM.MoveAbortedNotif(this);

        }

        if (pickupCoroutine != null) {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
            mM.PickupAbortedNotif(this);
        }

        //if (isLoadingSouls) {
        //    StopCoroutine(LoadingSouls());
        //    isLoadingSouls = false;
        //    Debug.Log("Coroutine stopped. Congratulations.");
        //}

    }

    // Function that changes and tracks the ship state (AtBase / Deployed) and notifies other scripts
    private void ChangeShipState(ShipState newState)
    {
        CurrentShipState = newState;

        switch (CurrentShipState)
        {
            //Enable the ship and all it's components
            case ShipState.Deployed:
                spriteRenderer.enabled = true;
                shipCollider.enabled = true;
                detectionZone.enabled = true;
                break;

            //Disable the ship and all it's components
            case ShipState.Unloading:
                spriteRenderer.enabled = false;
                shipCollider.enabled = false;
                detectionZone.enabled = false;
                StartCoroutine(UnloadSouls());
                break;

            case ShipState.AtBase:
                spriteRenderer.enabled = false;
                shipCollider.enabled = false;
                detectionZone.enabled = false;
                break;

            //Remove the ship from play
            case ShipState.Destroyed:
                DestroyShip();
                break;

            default:
                break;
        }

        // Action call when a ship changes state
        if (shipStateChange != null)
            shipStateChange(this);
    }

    // Function that changes name and callsign, and notifies other scripts
    private void ChangeShipName(string newName, string newCallsign)
    {
        shipName = newName;
        shipCallsign = newCallsign;

        // Force 3 characters Max (safety net)
        if (shipCallsign.Length > 3)
        {
            shipCallsign = shipCallsign.Substring(0, 3);
        }

        if (shipInfoChange != null)
            shipInfoChange(this);
    }

    // Simple function that toggles SpriteRender on/off, called from DebugManager-->ShipManager-->Ship
    public void ToggleSprite(bool toggleON)
    {
        spriteRenderer.enabled = toggleON;
    }

    private void DestroyShip()
    {
        if (linkedMarker != null)
            linkedMarker.DestroyMarker();

        if (shipDestroyed != null)
            shipDestroyed(this);

        spriteRenderer.enabled = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    // Safety net - When a ship is disabled, sends info to the ShipManager (TODO : Nothing calls DisableShip yet)
    // Function to be implemented when a ship is fully destroyed, or when the scene changes
    private void DisableShip()
    {
        if (linkedMarker != null)
            linkedMarker.DisableMarker();

        if (shipDisabled != null)
            shipDisabled(this);

        this.gameObject.SetActive(false);
    }

    private IEnumerator LoadingSouls() {

        PlanetSoulsMatch planetSoulsMatch = new PlanetSoulsMatch(0, planetInOrbit);

        while (planetInOrbit.CurrentSouls > 0 && currentCargo < cargoCapacity) {

            yield return new WaitForSeconds(pickupSpeedInSeconds);
            int addedSouls = planetInOrbit.RemoveSoul(1);
            currentCargo += addedSouls;
            planetSoulsMatch.soulsAmount += addedSouls; 
        }

        _allSoulsAndPlanets.Add(planetSoulsMatch);
        mM.PickupFinishedNotif(this);
    }

    // Function that teleports the ship to a targetwormhole, stops movement, 
    // assigns a transposed movement vector, and starts moving again
    public void EnterWormHole(WormHole targetWormHole)
    {
        // Get remaining movement
        Vector2 remainingMovement =  targetWorldCoords - (Vector2)transform.position;

        // Assign states and teleport ship
        isInWormHole = true;
        isMoving = false;
        transform.position = targetWormHole.transform.position;

        // Start a new movement vector
        Vector2 newTargetWorldCoords = (Vector2)transform.position + remainingMovement;

        // Get current grid info and assign max positions, according to game world bounds
        GridInfo currentGridInfo = GridCoords.CurrentGridInfo;
        if (currentGridInfo != null)
        {
            float minX = currentGridInfo.gameGridWorldBounds.min.x;
            float maxX = currentGridInfo.gameGridWorldBounds.max.x;
            float minY = currentGridInfo.gameGridWorldBounds.min.y;
            float maxY = currentGridInfo.gameGridWorldBounds.max.y;

            newTargetWorldCoords.x = Mathf.Clamp(newTargetWorldCoords.x, minX, maxX);
            newTargetWorldCoords.y = Mathf.Clamp(newTargetWorldCoords.y, minY, maxY);
        }

        // Assign new vector and start moving again
        targetWorldCoords = newTargetWorldCoords;
        isMoving = true;

        Debug.Log("I am in a wormhole");
    }

    // Verify, on everyframe, if ship is in anomaly tile
    private void CheckForAnomaly()
    {
        // If ship is in an anomaly tile
        if (anomalyTile != null)
        {
            // If no damage routine yet
            if (damageTickCoroutine == null)
            {
                damageTickCoroutine = DamageTick();
                StartCoroutine(damageTickCoroutine);
            }
        }
        // If ship is not in anomaly tile
        else
        {
            // If damage routine is active
            if (damageTickCoroutine != null)
            {
                StopCoroutine(damageTickCoroutine);
                damageTickCoroutine = null;
            }
        }
    }

    private IEnumerator DamageTick()
    {
        //Debug.Log("DAMAGE STARTED");
        while (currentHealthPoints > 0)
        {
            if (anomalyTile == null)
                break;

            yield return new WaitForSeconds(1f);
            currentHealthPoints -= (int)anomalyTile.shipDPS;
        }

        currentHealthPoints = 0;
        ChangeShipState(ShipState.Destroyed);
        damageTickCoroutine = null;
        yield return null;
    }

    private IEnumerator UnloadSouls()
    {
        while (currentCargo > 0)
        {
            yield return new WaitForSeconds(1f);
            int previousSouls = currentCargo;
            currentCargo = Mathf.Clamp(currentCargo - soulsUnloadedPerSec, 0, cargoCapacity);

            int soulsRemoved = previousSouls - currentCargo;

            if (soulsUnloaded != null)
                soulsUnloaded(soulsRemoved);
        }

        foreach (var match in _allSoulsAndPlanets)
        {
            if (soulsFromPlanetSaved != null)
                soulsFromPlanetSaved(match);

            Debug.Log("Sending match action with souls :  " + match.soulsAmount);
        }
        _allSoulsAndPlanets.Clear();

        ChangeShipState(ShipState.AtBase);
    }
}

public enum ShipState
{
    Deployed,
    Unloading,
    AtBase,
    Destroyed
}

