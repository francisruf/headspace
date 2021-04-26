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
    public static Action<Ship> shipRemoved;
    public static Action<Ship> shipDisabled;
    public static Action<Ship> shipReEnabled;
    public static Action<Ship> shipStateChange;
    public static Action<Ship> shipInfoChange;
    public static Action<int> soulsUnloaded;
    public static Action<Ship> routeFinished;
    public static Action<PlanetSoulsMatch> soulsFromPlanetSaved;

    // Components
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D shipCollider;
    private CircleCollider2D detectionZone;

    // Linked Marker
    private ShipMarker linkedMarker;
    public GameObject markerPrefab;

    //STATS
    [Header("Info")]
    public string shipName;
    public string shipCallsign;
    public string shipClass;

    [Header("Stats")]

    [Range(0, 100)]
    public int healthPoints;
    public int currentHealthPoints;

    [Range(0, 4)]
    //How many souls can the Ship carry
    public int cargoCapacity;
    [HideInInspector] public int currentCargo;

    [Range(0, 100)]
    public float tileTravelSpeed;

    [Range(0, 120)]
    public float disabledCooldownTime;

    [Range(0, 10)]
    //How many seconds to pickup ONE Soul
    public float pickupSpeedInSeconds;

    [Range(0, 10)]
    public int soulsUnloadedPerSec;

    [Range(0, 1)]
    public float detectionRadius;

    // CONTRACT INFO
    private List<Contract> _assignedContracts = new List<Contract>();
    private List<Client> _clientsOnBoard = new List<Client>();
    public List<Client> ClientsOnBoard { get { return _clientsOnBoard; } }

    //MOVEMENT
    private Vector2 displayedGridCoords;
    [HideInInspector] public Vector2 targetWorldCoords;
    [HideInInspector] public bool isMoving;

    public List<string> Destinations { get; private set; }
    private Queue<string> _currentDestinations = new Queue<string>();
    private IEnumerator _currentRoute;
    private IEnumerator _currentMove;

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

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    private void Awake()
    {
        // Assign component references
        spriteRenderer = GetComponent<SpriteRenderer>();
        shipCollider = GetComponentInChildren<PolygonCollider2D>();
        detectionZone = GetComponentInChildren<CircleCollider2D>();
    }

    void Start()
    {
        if (DebugManager.instance != null)
        {
            spriteRenderer.enabled = DebugManager.instance.DebugObjectsVisible;
        }
        else
        {
            spriteRenderer.enabled = false;
        }

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

        ////Assign basePosition
        ////basePosition = new Vector2(100f, 100f);
        //basePosition = new Vector2(0f, 0f); //ONLY FOR TESTING

        ////Start ships at basePosition
        //transform.position = basePosition;

        //Fire the newShipAvaible action (received by the ShipManager)
        if (newShipAvailable != null)
            newShipAvailable(this);

        mM = MessageManager.instance;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.M))
        //{
        //    StartShipMove();
        //}

        //MoveShip();

        //Finds current position at all times in Grid Coords
        //currentPositionInGridCoords = GridCoords.FromWorldToGrid(transform.position);
    }

    public void StartNewRoute(List<string> route)
    {
        Destinations = route;
        StartShipMove();
    }

    private void StartShipMove()
    {
        if (_currentRoute != null)
        {
            StopCoroutine(_currentRoute);
            _currentRoute = null;

            if (_currentMove != null)
            {
                StopCoroutine(_currentMove);
                _currentMove = null;
            }
        }

        _currentDestinations.Clear();

        for (int i = 0; i < Destinations.Count; i++)
        {
            _currentDestinations.Enqueue(Destinations[i]);
        }

        _currentRoute = ExecuteRoute();
        StartCoroutine(_currentRoute);
    }

    private IEnumerator ExecuteRoute()
    {
        ChangeShipState(ShipState.Busy);
        bool error = false;
        while (_currentDestinations.Count > 0)
        {
            string newDest = _currentDestinations.Dequeue();
            TileCoordinates destCoords;
            if (ValidateDestination(newDest, out destCoords))
            {
                _currentMove = MoveToDestination(destCoords, newDest);
                yield return StartCoroutine(_currentMove);
            }
            else
            {
                CancelRoute("Invalid destination - " + newDest);
                error = true;
                break;
            }
        }
        ChangeShipState(ShipState.Idle);
        mM.RouteFinishedNotif(this);

        yield return new WaitForFixedUpdate();

        if (CurrentShipState != ShipState.Disabled)
        {
            if (routeFinished != null)
                routeFinished(this);
            Debug.Log("NOTIF");
        }


        _currentRoute = null;
    }

    private bool ValidateDestination(string dest, out TileCoordinates destCoords)
    {
        destCoords = new TileCoordinates(0, 0);
        bool valid = GridCoords.FromTileNameToTilePosition(dest, out destCoords);

        return valid;
    }

    private IEnumerator MoveToDestination(TileCoordinates destCoords, string destName)
    {
        if (PathFinder.instance == null)
            yield break;

        TileCoordinates currentTileCoords = GridCoords.FromWorldToTilePosition(transform.position);
        float nodeCost = 0f;
        List<PathNode> pathNodes = PathFinder.instance.FindLinearPath(currentTileCoords.tileX, currentTileCoords.tileY, destCoords.tileX, destCoords.tileY, out nodeCost);
        if (pathNodes == null)
        {
            CancelRoute("Invalid path to destination - " + destName);
            yield break;
        }

        List<Vector2> pathPositions = new List<Vector2>();
        foreach (var node in pathNodes)
            pathPositions.Add(node.tile.TileCenter);

        PathNode previousNode = null;
        for (int i = 0; i < pathNodes.Count; i++)
        {
            // IF HIT A ROCK
            if (!pathNodes[i].isTraversable)
            {
                CancelRoute("Obstacle in the way.");
                break;
            }

            //if (previousNode != null)
            //    if (!PathFinder.instance.GetValidNeighbourList(previousNode, false).Contains(pathNodes[i]))
            //    {
            //        CancelRoute("DIAGONAL OBSTACLE IN THE WAY!");
            //        break;
            //    }

            previousNode = pathNodes[i];

            if (i == 0)
                continue;

            yield return new WaitForSeconds(tileTravelSpeed * nodeCost);
            transform.position = pathPositions[i];
        }
        _currentMove = null;
        yield return null;
    }

    private bool CancelRoute(string errorMessage = "", bool error = true)
    {
        bool cancelled = false;

        _currentDestinations.Clear();

        if (_currentRoute != null)
        {
            StopCoroutine(_currentRoute);
            _currentRoute = null;
            cancelled = true;
        }
        if (_currentMove != null)
        {
            StopCoroutine(_currentMove);
            _currentMove = null;
            cancelled = true;
        }

        ChangeShipState(ShipState.Idle);

        if (error)
            mM.InvalidDestinationNotif(this, errorMessage);

        return cancelled;
    }

    private void MoveShip()
    {
        //If MOVE command is called, moves the ship at coordinates indicated in the MOVE command.
        if (isMoving)
        {
            //Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Move " + displayedGridCoords + " | STATUS: Moving");
            transform.position = Vector2.MoveTowards(transform.position, targetWorldCoords, tileTravelSpeed * Time.deltaTime);

            if (targetWorldCoords == (Vector2)transform.position)
            {
                isMoving = false;
                //Debug.Log("Movement has ended");

                mM.MoveFinishedNotif(this);

                //if (planetInOrbit != null)
                //{
                //    //mM.EnteredPlanetOrbitNotif(this, planetInOrbit);
                //}

                //else
                //{
                //    mM.MoveFinishedNotif(this);
                //}
            }
        }
    }


    // Function that initializes ship parameters when instantiated
    public void InitializeShip(string shipName, string shipCallsign, ShipState startingState)
    {
        shipStartingState = startingState;
        ChangeShipName(shipName, shipCallsign);
        ChangeShipState(startingState);

        SpawnMarker();
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

        ////Stops function if Ship cannot be Deployed
        //if (CurrentShipState == ShipState.Deployed) {
        //    Debug.Log("Ship already Deployed");
        //    return;
        //}

        ////Stops function if Ship cannot be Deployed
        //if (CurrentShipState == ShipState.Unloading)
        //{
        //    Debug.Log("Ship is still unloading");
        //    return;
        //}

        ////Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Deploy " + gridCoords + " | STATUS: Deployed");

        ////Convert gridCoords entered into worldCoords
        //targetWorldCoords = GridCoords.FromGridToWorld(gridCoords);
        ////Place ship on the entered coordinates
        //transform.position = targetWorldCoords;
        //currentPositionInGridCoords = GridCoords.FromWorldToGrid(transform.position);

        ////Change the status of the ship from "At Base" to "Deployed"
        //ChangeShipState(ShipState.Deployed);

        ////Notify player that Ship has Deployed
        //mM.ShipDeployedNotif(this);
    }

    public void Leave() {

        ////Stops function if Ship cannot be Leave
        //if (CurrentShipState != ShipState.Deployed) {
        //    Debug.Log("Ship is already At Base");
        //    return;
        //}

        //if (isMoving)
        //    isMoving = false;

        ////Debug.Log("SHIP NAME: " + shipName + " | COMMAND: Leave | STATUS: On it's way to base");


        ////Place ship on the entered coordinates
        //transform.position = basePosition;

        //ChangeShipState(ShipState.Unloading);
    }

    public void Move(Vector2 gridCoords){

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

        bool cancelled = CancelRoute("", false);
        mM.ShipStopMovementNotif(this, cancelled);


        //if (isLoadingSouls) {
        //    StopCoroutine(LoadingSouls());
        //    isLoadingSouls = false;
        //    Debug.Log("Coroutine stopped. Congratulations.");
        //}
    }

    public void Status()
    {
        mM.ShipStatusNotif(this);
    }

    // Function that changes and tracks the ship state (AtBase / Deployed) and notifies other scripts
    private void ChangeShipState(ShipState newState)
    {
        CurrentShipState = newState;

        switch (CurrentShipState)
        {
            //Enable the ship and all it's components
            case ShipState.Idle:
                break;

            //Disable the ship and all it's components
            case ShipState.Busy:
                break;

            //Remove the ship from play
            case ShipState.Disabled:
                break;

            default:
                break;
        }

        // Action call when a ship changes state
        if (shipStateChange != null)
            shipStateChange(this);
    }

    // Function that changes name and callsign, and notifies other scripts
    public void ChangeShipName(string newName, string newCallsign)
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

        if (shipRemoved != null)
            shipRemoved(this);

        spriteRenderer.enabled = false;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    // Safety net - When a ship is disabled, sends info to the ShipManager (TODO : Nothing calls DisableShip yet)
    // Function to be implemented when a ship is fully destroyed, or when the scene changes
    private void RemoveShipFromFGame()
    {
        if (linkedMarker != null)
            linkedMarker.DisableObject();

        if (shipRemoved != null)
            shipRemoved(this);

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
    //private void CheckForAnomaly()
    //{
    //    // If ship is in an anomaly tile
    //    if (anomalyTile != null)
    //    {
    //        // If no damage routine yet
    //        if (damageTickCoroutine == null)
    //        {
    //            damageTickCoroutine = DamageTick();
    //            StartCoroutine(damageTickCoroutine);
    //        }
    //    }
    //    // If ship is not in anomaly tile
    //    else
    //    {
    //        // If damage routine is active
    //        if (damageTickCoroutine != null)
    //        {
    //            StopCoroutine(damageTickCoroutine);
    //            damageTickCoroutine = null;
    //        }
    //    }
    //}

    //private IEnumerator DamageTick()
    //{
    //    //Debug.Log("DAMAGE STARTED");
    //    while (currentHealthPoints > 0)
    //    {
    //        if (anomalyTile == null)
    //            break;

    //        yield return new WaitForSeconds(1f);
    //        currentHealthPoints -= (int)anomalyTile.shipDPS;
    //    }

    //    currentHealthPoints = 0;
    //    ChangeShipState(ShipState.Disabled);
    //    damageTickCoroutine = null;
    //    yield return null;
    //}

    //private IEnumerator UnloadSouls()
    //{
    //    while (currentCargo > 0)
    //    {
    //        yield return new WaitForSeconds(1f);
    //        int previousSouls = currentCargo;
    //        currentCargo = Mathf.Clamp(currentCargo - soulsUnloadedPerSec, 0, cargoCapacity);

    //        int soulsRemoved = previousSouls - currentCargo;

    //        if (soulsUnloaded != null)
    //            soulsUnloaded(soulsRemoved);
    //    }

    //    foreach (var match in _allSoulsAndPlanets)
    //    {
    //        if (soulsFromPlanetSaved != null)
    //            soulsFromPlanetSaved(match);

    //        Debug.Log("Sending match action with souls :  " + match.soulsAmount);
    //    }
    //    _allSoulsAndPlanets.Clear();

    //    //ChangeShipState(ShipState.AtBase);
    //}

    public void AssignContract(Contract contract)
    {
        _assignedContracts.Add(contract);
        CheckForClients();
    }

    public void RemoveContract(Contract contract)
    {
        _assignedContracts.Remove(contract);
    }

    private void CheckForClients()
    {
        TileCoordinates currentTile = GridCoords.FromWorldToTilePosition(transform.position);
        GridTile tile = GridCoords.CurrentGridInfo.gameGridTiles[currentTile.tileX, currentTile.tileY];
        GridTile_Planet planet = tile.GetComponent<GridTile_Planet>();

        if (planet != null)
            CheckForClients(planet);
    }

    public void CheckForClients(GridTile_Planet planet)
    {
        List<Client> clientsList = new List<Client>();
        // Check for clients on contracts that can be picked up, if planet is right
        foreach (var contract in _assignedContracts)
        {
            foreach (var client in contract.AllClients)
            {
                if (client.currentState == ClientState.Waiting && client.startPlanet == planet)
                    clientsList.Add(client);
            }
        }
        foreach (var client in clientsList)
        {
            EmbarkClient(client);
        }
        clientsList.Clear();

        // Check for onboard clients that can debark
        foreach (var client in _clientsOnBoard)
        {
            if (client.CheckSuccessCondition(planet))
            {
                clientsList.Add(client);
            }
        }

        foreach (var client in clientsList)
        {
            DebarkClient(client);
        }
    }

    private void EmbarkClient(Client client)
    {
        if (!_clientsOnBoard.Contains(client))
        {
            if (currentCargo < cargoCapacity)
            {
                _clientsOnBoard.Add(client);
                client.ChangeState(ClientState.Embarked);
                currentCargo++;
            }
        }
        linkedMarker.UpdateLights(currentCargo);
    }

    private void DebarkClient(Client client)
    {
        if (_clientsOnBoard.Remove(client))
        {
            client.ChangeState(ClientState.Debarked);
            currentCargo--;
        }
        linkedMarker.UpdateLights(currentCargo);
    }

    public void DisableShip()
    {
        CancelRoute("", false);

        int count = 0;
        foreach (var client in _clientsOnBoard)
        {
            client.ChangeState(ClientState.Dead);
            currentCargo--;
            count++;
        }

        linkedMarker.UpdateLights(currentCargo);
        _clientsOnBoard.Clear();
        mM.ShipAnomalyNotif(this, count);
        StartCoroutine(DisableCooldown());

        if (shipDisabled != null)
            shipDisabled(this);
    }

    private IEnumerator DisableCooldown()
    {
        ChangeShipState(ShipState.Disabled);
        yield return new WaitForSeconds(disabledCooldownTime);
        ChangeShipState(ShipState.Idle);
        mM.ShipResetNotif(this);

        if (shipReEnabled != null)
            shipReEnabled(this);
    }
}

public enum ShipState
{
    Idle,
    Busy,
    Disabled
}

