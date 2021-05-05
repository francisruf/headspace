using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractManager : MonoBehaviour
{
    public static Action newContractReceived;
    public static ContractManager instance;

    [Header("Spawn settings")]
    public float firstContractSpawnTime;
    private float _defaultContractSpawnInterval;

    [Header("Prefabs")]
    public GameObject singleContractPrefab;
    public GameObject doubleContractPrefab;

    [Header("Points settings")]
    public ContractPointsSettings pointSettings;

    private ContractsDB _contractsDB;
    private ConveyorBelt _belt;

    private List<Contract> _allContracts = new List<Contract>();
    public List<Contract> CurrentContracts { get { return _allContracts; } }

    private List<ClientRules> _clientRules;
    private int _currentRuleIndex;
    private bool _allRulesApplied;
    private int _doubleContractChance;
    private int _tripleContractChance;
    private int _spawnCount;

    private IEnumerator _spawnRoutine;
    private IEnumerator _newContractRoutine;

    private ContractSpawnCondition _currentSpawnCondition;

    private void Awake()
    {
        // Assigner le singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        _contractsDB = GetComponentInChildren<ContractsDB>();
    }

    private void Start()
    {
        _belt = FindObjectOfType<ConveyorBelt>();
    }

    private void OnEnable()
    {
        GameManager.levelStarted += OnGameStarted;
        TimeManager.thirtyMinsWarning += StopSpawning;
    }

    private void OnDisable()
    {
        GameManager.levelStarted -= OnGameStarted;
        TimeManager.thirtyMinsWarning -= StopSpawning;
    }

    private void StopSpawning()
    {
        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);
        if (_newContractRoutine != null)
            StopCoroutine(_newContractRoutine);

        _spawnRoutine = null;
        _newContractRoutine = null;
    }

    public void AssignLevelSettings(List<ClientRules> clientRules, ContractSpawnCondition condition, float defaultSpawnInterval)
    {
        this._currentSpawnCondition = condition;
        this._clientRules = clientRules;
        this._defaultContractSpawnInterval = defaultSpawnInterval;
        Debug.Log("Received client rules");
    }

    private void OnGameStarted()
    {
        if (_currentSpawnCondition == ContractSpawnCondition.Timed)
        {
            _newContractRoutine = NewContractTimer();
            StartCoroutine(_newContractRoutine);
        }
    }

    public void TriggerNextContract()
    {
        StartCoroutine(SpawnContractTimer(0f));
    }

    public void ChangeContractConditions(ContractSpawnCondition newConditions)
    {
        _currentSpawnCondition = newConditions;

        if (_currentSpawnCondition == ContractSpawnCondition.Timed)
        {
            if (_newContractRoutine == null)
            {
                _newContractRoutine = NewContractTimer();
                StartCoroutine(_newContractRoutine);
            }
        }
    }

    // TODO : Wrapper ça dans container de contrat de grosseur différente
    public void CreateNewSingleContract()
    {
        List<Client> allClients = new List<Client>();
        int contractSize = 1;
        MovableContract movableContract = null;

        // Contract object
        if (contractSize == 1)
            movableContract = Instantiate(singleContractPrefab).GetComponent<MovableContract>();

        //else if (contractSize == 2)
        //    movableContract = Instantiate(doubleContractPrefab).GetComponent<MovableContract>();

        Vector2 spawnPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        if (_belt == null)
            _belt = FindObjectOfType<ConveyorBelt>();

        if (_belt != null)
        {
            spawnPos = _belt.contractsStartPos.position;
            spawnPos.x = _belt.Bounds.center.x;
            spawnPos.x -= movableContract.ObjSpriteRenderer.bounds.extents.x;

            endPos.x = spawnPos.x;
            endPos.y = _belt.contractsEndPos.position.y;
            endPos.y += movableContract.ObjSpriteRenderer.bounds.size.y;
            endPos.y += 2 * (1 / 32f); // 2px offset

            movableContract.SetSortingLayer(_belt.SpriteRenderer.sortingLayerID);
            movableContract.SetOrderInLayer(_belt.SpriteRenderer.sortingOrder + 1);
        }

        movableContract.transform.position = spawnPos;
        movableContract.InitializeContract(endPos);
        movableContract.ClientAmount = contractSize;

        // Client settings
        // Start planet
        List<GridTile_Planet> allCandidatePlanets = new List<GridTile_Planet>(PlanetManager.instance.AllPlanetTiles);
        GridTile_Planet startPlanet = GetStartPlanet(allCandidatePlanets);

        if (startPlanet == null)
        {
            Debug.LogWarning("Could not find target start planet. Check start settings.");
            int randomIndex = UnityEngine.Random.Range(0, allCandidatePlanets.Count);
            startPlanet = allCandidatePlanets[randomIndex];
        }

        for (int i = 0; i < contractSize; i++)
        {
            // End planet
            //List<GridTile_Planet> distanceCandidates = GetPlanetsByDistance(allCandidatePlanets, candidateEndPlanets, startPlanet);
            GridTile_Planet endPlanet = GetEndPlanet(allCandidatePlanets, startPlanet);

            allClients.Add(CreateClient(startPlanet, endPlanet));
            allCandidatePlanets.Remove(endPlanet);
            int ruleCount = _clientRules.Count;

            if (_currentRuleIndex == _clientRules.Count - 1)
                _allRulesApplied = true;

            if (!_allRulesApplied)
                _currentRuleIndex++;
            else
                _currentRuleIndex = UnityEngine.Random.Range(0, ruleCount);

            startPlanet.AddContractHeat(1);
        }

        if (contractSize == 1)
        {
            Contract_Single contract = movableContract.GetComponent<Contract_Single>();
            contract.AssignClients(allClients);
            contract.CalculatePointsReward(pointSettings);
            _allContracts.Add(contract);
        }
        //else if (contractSize == 2)
        //{
        //    Contract_Double contract = movableContract.GetComponent<Contract_Double>();
        //    contract.AssignClients(allClients);
        //    contract.CalculatePointsReward(pointSettings);
        //    _allContracts.Add(contract);
        //}

        _spawnCount++;

        if (newContractReceived != null)
            newContractReceived();
    }

    private GridTile_Planet GetRandomPlanet(List<GridTile_Planet> targetList)
    {
        int randomIndex = UnityEngine.Random.Range(0, targetList.Count);
        GridTile_Planet target = targetList[randomIndex];
        return target;
    }

    private GridTile_Planet GetStartPlanet(List<GridTile_Planet> completeList)
    {
        if (_clientRules[_currentRuleIndex].specialStartCondition == SpecialConditions.ClosestPlanet)
        {
            return GetClosestStartPlanet(completeList);
        }
        else if (_clientRules[_currentRuleIndex].startDistanceRating == 0)
        {
            List<GridTile_Planet> candidateStartPlanets = GetStartPlanetsByHeat(completeList);
            return GetRandomPlanet(candidateStartPlanets);
        }
        else
        {
            List<GridTile_Planet> candidateStartPlanets = GetStartPlanetByDistance(completeList);
            return GetRandomPlanet(candidateStartPlanets);
        }
    }

    private GridTile_Planet GetClosestStartPlanet(List<GridTile_Planet> completeList)
    {
        if (ShipManager.instance == null)
            return null;

        List<Ship> allShips = ShipManager.instance.AllShips;
        int count = completeList.Count;
        int targetIndex = int.MaxValue;
        int minRating = int.MaxValue;

        foreach (var ship in allShips)
        {
            TileCoordinates shipPos = GridCoords.FromWorldToTilePosition(ship.transform.position);

            for (int i = 0; i < count; i++)
            {
                TileCoordinates planetPos = completeList[i].TileCoordinates;
                if (shipPos != planetPos)
                {
                    int distanceRating = PathFinder.instance.GetDistanceRating(shipPos.tileX, shipPos.tileY, planetPos.tileX, planetPos.tileY, true);
                    if (distanceRating < minRating)
                    {
                        minRating = distanceRating;
                        targetIndex = i;
                    }
                    //Debug.Log("Distance from " + ship.shipName + " to " + completeList[i].PlanetName + " : " + distanceRating);
                }
            }
        }

        if (targetIndex < count)
        {
            Debug.Log("CLOSEST PLANET : " + completeList[targetIndex].PlanetName);

            return completeList[targetIndex];
        }

        return null;
    }

    private List<GridTile_Planet> GetStartPlanetsByHeat(List<GridTile_Planet> completeList)
    {
        //StartPlanetState startState = _clientRules[_currentRuleIndex].startPlanetState;
        int count = 0;
        int minHeat = int.MaxValue;
        List<GridTile_Planet> candidatePlanets = new List<GridTile_Planet>();

        foreach (var planet in completeList)
        {
            if (planet.ContractHeat < minHeat)
            {
                minHeat = planet.ContractHeat;
            }
        }

        foreach (var planet in completeList)
        {
            int RandomChance = UnityEngine.Random.Range(0, 2);
            if (planet.ContractHeat <= minHeat + RandomChance)
            {
                candidatePlanets.Add(planet);
                count++;
            }
        }

        if (count > 0)
            return candidatePlanets;
        else
            return new List<GridTile_Planet>(completeList);
    }

    private List<GridTile_Planet> GetStartPlanetByDistance(List<GridTile_Planet> completeList)
    {
        if (ShipManager.instance == null)
            return null;

        List<int> planetDistances = new List<int>();
        List<Ship> allShips = ShipManager.instance.AllShips;
        int count = 0;

        foreach (var planet in completeList)
        {
            TileCoordinates planetPos = planet.TileCoordinates;
            int minDistance = int.MaxValue;

            foreach (var ship in allShips)
            {
                TileCoordinates shipPos = GridCoords.FromWorldToTilePosition(ship.transform.position);
                if (shipPos != planetPos)
                {
                    int distanceRating = PathFinder.instance.GetDistanceRating(shipPos.tileX, shipPos.tileY, planetPos.tileX, planetPos.tileY, true);
                    if (distanceRating < minDistance)
                        minDistance = distanceRating;
                }
            }
            planet.CurrentDistanceRating = minDistance;
            planetDistances.Add(minDistance);
            count++;
        }
        planetDistances.Sort();

        int targetDistanceRating = _clientRules[_currentRuleIndex].startDistanceRating;

        List<GridTile_Planet> shortDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> mediumDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> longDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> candidates = new List<GridTile_Planet>();
        int shortCount = 0;
        int mediumCount = 0;
        int longCount = 0;

        int third = planetDistances[(count / 3) - 1];
        int twoThirds = planetDistances[(count / 3 * 2) - 1];

        foreach (var planet in completeList)
        {
            if (planet.CurrentDistanceRating <= third)
            {
                Debug.Log("SHORT DIST PLANET : " + planet.PlanetName);
                shortDistancePlanets.Add(planet);
                shortCount++;
            }
            else if (planet.CurrentDistanceRating > twoThirds)
            {
                Debug.Log("MEDIUM DIST PLANET : " + planet.PlanetName);
                longDistancePlanets.Add(planet);
                longCount++;
            }
            else
            {
                Debug.Log("LONG DIST PLANET : " + planet.PlanetName);
                mediumDistancePlanets.Add(planet);
                mediumCount++;
            }
        }

        if (targetDistanceRating == 1)
        {
            if (shortCount > 0)
                candidates = shortDistancePlanets;

            else if (mediumCount > 0)
                candidates = mediumDistancePlanets;

            else if (longCount > 0)
                candidates = longDistancePlanets;

            else
            {
                candidates = new List<GridTile_Planet>(completeList);
            }
        }

        else if (targetDistanceRating == 2)
        {
            if (mediumCount > 0)
                candidates = mediumDistancePlanets;
            else
            {
                List<GridTile_Planet> shortLong = new List<GridTile_Planet>();
                int shortLongCount = 0;
                foreach (var planet in shortDistancePlanets)
                {
                    shortLong.Add(planet);
                    shortLongCount++;
                }
                foreach (var planet in longDistancePlanets)
                {
                    shortLong.Add(planet);
                    shortLongCount++;
                }

                if (shortLongCount > 0)
                {
                    candidates = shortLong;
                }
                else
                {
                    candidates = new List<GridTile_Planet>(completeList);
                }
            }

        }
        else if (targetDistanceRating == 3)
        {
            if (longCount > 0)
                candidates = longDistancePlanets;

            else if (mediumCount > 0)
                candidates = mediumDistancePlanets;

            else if (shortCount > 0)
                candidates = shortDistancePlanets;

            else
            {
                candidates = new List<GridTile_Planet>(completeList);
            }
        }
        return candidates;
    }

    private GridTile_Planet GetEndPlanet(List<GridTile_Planet> completeList, GridTile_Planet startPlanet)
    {
        if (_clientRules[_currentRuleIndex].specialEndCondition == SpecialConditions.ClosestPlanet)
        {
            return GetClosestEndPlanet(completeList, startPlanet);
        }
        else if (_clientRules[_currentRuleIndex].travelDistanceRating == 0)
        {
            return GetRandomEndPlanet(completeList, startPlanet);
        }
        else
        {
            List<GridTile_Planet> candidates = GetEndPlanetsByDistance(completeList, startPlanet);
            return GetRandomPlanet(candidates);
        }
    }

    private GridTile_Planet GetClosestEndPlanet(List<GridTile_Planet> completeList, GridTile_Planet startPlanet)
    {
        int count = completeList.Count;
        int targetIndex = int.MaxValue;
        int minRating = int.MaxValue;

        TileCoordinates startPos = startPlanet.TileCoordinates;
        for (int i = 0; i < count; i++)
        {
            if (completeList[i] != startPlanet)
            {
                TileCoordinates planetPos = completeList[i].TileCoordinates;
                int distanceRating = PathFinder.instance.GetDistanceRating(startPos.tileX, startPos.tileY, planetPos.tileX, planetPos.tileY, true);
                if (distanceRating < minRating)
                {
                    minRating = distanceRating;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex < count)
        {
            //Debug.Log("CLOSEST PLANET : " + completeList[targetIndex].PlanetName);
            return completeList[targetIndex];
        }

        return null;
    }

    private GridTile_Planet GetRandomEndPlanet(List<GridTile_Planet> completeList, GridTile_Planet startPlanet)
    {
        List<GridTile_Planet> candidates = new List<GridTile_Planet>(completeList);
        candidates.Remove(startPlanet);

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private List<GridTile_Planet> GetEndPlanetsByDistance(List<GridTile_Planet> completeList, GridTile_Planet startPlanet)
    {
        List<int> planetDistances = new List<int>();
        List<Ship> allShips = ShipManager.instance.AllShips;
        int count = 0;

        foreach (var planet in completeList)
        {
            if (planet == startPlanet)
                continue;

            TileCoordinates planetPos = planet.TileCoordinates;
            TileCoordinates startPos = startPlanet.TileCoordinates;
            int distanceRating = PathFinder.instance.GetDistanceRating(startPos.tileX, startPos.tileY, planetPos.tileX, planetPos.tileY, true);

            planet.CurrentDistanceRating = distanceRating;
            planetDistances.Add(distanceRating);
            count++;
        }
        planetDistances.Sort();

        // CHANGE THIS
        int targetDistanceRating = _clientRules[_currentRuleIndex].travelDistanceRating;

        List<GridTile_Planet> shortDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> mediumDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> longDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> candidates = new List<GridTile_Planet>();
        int shortCount = 0;
        int mediumCount = 0;
        int longCount = 0;

        int third = planetDistances[(count / 3) - 1];
        int twoThirds = planetDistances[(count / 3 * 2) - 1];
        //Debug.Log("----- END PLANETS 1/3 DIST : " + third);
        //Debug.Log("----- END PLANETS 2/3 DIST : " + twoThirds);


        foreach (var planet in completeList)
        {
            if (planet == startPlanet)
                continue;

            if (planet.CurrentDistanceRating < third)
            {
                //Debug.Log("SHORT DIST PLANET (" + planet.CurrentDistanceRating + ") : " + planet.PlanetName);
                shortDistancePlanets.Add(planet);
                shortCount++;
            }
            else if (planet.CurrentDistanceRating >= twoThirds)
            {
                //Debug.Log("LONG DIST PLANET (" + planet.CurrentDistanceRating + ") : " + planet.PlanetName);
                longDistancePlanets.Add(planet);
                longCount++;
            }
            else
            {
                //Debug.Log("MEDIUM DIST PLANET (" + planet.CurrentDistanceRating + ") : " + planet.PlanetName);
                mediumDistancePlanets.Add(planet);
                mediumCount++;
            }
        }

        if (targetDistanceRating == 1)
        {
            if (shortCount > 0)
                candidates = shortDistancePlanets;

            else if (mediumCount > 0)
                candidates = mediumDistancePlanets;

            else if (longCount > 0)
                candidates = longDistancePlanets;

            else
            {
                candidates = new List<GridTile_Planet>(completeList);
                candidates.Remove(startPlanet);
            }
        }

        else if (targetDistanceRating == 2)
        {
            if (mediumCount > 0)
                candidates = mediumDistancePlanets;
            else
            {
                List<GridTile_Planet> shortLong = new List<GridTile_Planet>();
                int shortLongCount = 0;
                foreach (var planet in shortDistancePlanets)
                {
                    shortLong.Add(planet);
                    shortLongCount++;
                }
                foreach (var planet in longDistancePlanets)
                {
                    shortLong.Add(planet);
                    shortLongCount++;
                }

                if (shortLongCount > 0)
                {
                    candidates = shortLong;
                }
                else
                {
                    candidates = new List<GridTile_Planet>(completeList);
                    candidates.Remove(startPlanet);
                }
            }

        }
        else if (targetDistanceRating == 3)
        {
            if (longCount > 0)
                candidates = longDistancePlanets;

            else if (mediumCount > 0)
                candidates = mediumDistancePlanets;

            else if (shortCount > 0)
                candidates = shortDistancePlanets;

            else
            {
                candidates = new List<GridTile_Planet>(completeList);
                candidates.Remove(startPlanet);
            }
        }
        return candidates;
    }

    //private List<GridTile_Planet> GetPlanetsByDistance(List<GridTile_Planet> completeList, List<GridTile_Planet> listToModify, GridTile_Planet startPlanet)
    //{
    //    int targetDistanceRating = _clientRules[_currentRuleIndex].distanceRating;

    //    List<GridTile_Planet> shortDistancePlanets = new List<GridTile_Planet>();
    //    List<GridTile_Planet> mediumDistancePlanets = new List<GridTile_Planet>();
    //    List<GridTile_Planet> longDistancePlanets = new List<GridTile_Planet>();
    //    List<GridTile_Planet> candidates = new List<GridTile_Planet>();
    //    int shortCount = 0;
    //    int mediumCount = 0;
    //    int longCount = 0;

    //    List<int> distances = new List<int>();
    //    int count = 0;
    //    foreach (var planet in completeList)
    //    {
    //        distances.Add(planet.DistanceRating);
    //        count++;
    //    }
    //    distances.Sort();

    //    for (int i = 0; i < count; i++)
    //    {
    //        Debug.Log(distances[i]);
    //    }

    //    int third = distances[(count / 3) - 1];
    //    int twoThirds = distances[(count / 3 * 2) - 1];

    //    foreach (var planet in listToModify)
    //    {
    //        if (planet.DistanceRating <= third)
    //        {
    //            Debug.Log("SHORT DIST PLANET : " + planet.PlanetName);
    //            shortDistancePlanets.Add(planet);
    //            shortCount++;
    //        }
    //        else if (planet.DistanceRating > twoThirds)
    //        {
    //            longDistancePlanets.Add(planet);
    //            longCount++;
    //        }
    //        else
    //        {
    //            mediumDistancePlanets.Add(planet);
    //            mediumCount++;
    //        }
    //    }

    //    if (targetDistanceRating == 1)
    //    {
    //        if (shortCount > 0)
    //            candidates = shortDistancePlanets;

    //        else if (mediumCount > 0)
    //            candidates = mediumDistancePlanets;

    //        else if (longCount > 0)
    //            candidates = longDistancePlanets;

    //        else
    //        {
    //            candidates = new List<GridTile_Planet>(completeList);
    //            candidates.Remove(startPlanet);
    //        }
    //    }

    //    else if (targetDistanceRating == 2)
    //    {
    //        if (mediumCount > 0)
    //            candidates = mediumDistancePlanets;
    //        else
    //        {
    //            List<GridTile_Planet> shortLong = new List<GridTile_Planet>();
    //            int shortLongCount = 0;
    //            foreach (var planet in shortDistancePlanets)
    //            {
    //                shortLong.Add(planet);
    //                shortLongCount++;
    //            }
    //            foreach (var planet in longDistancePlanets)
    //            {
    //                shortLong.Add(planet);
    //                shortLongCount++;
    //            }

    //            if (shortLongCount > 0)
    //            {
    //                candidates = shortLong;
    //            }
    //            else
    //            {
    //                candidates = new List<GridTile_Planet>(completeList);
    //                candidates.Remove(startPlanet);
    //            }
    //        }

    //    }
    //    else if (targetDistanceRating == 3)
    //    {
    //        if (longCount > 0)
    //            candidates = longDistancePlanets;

    //        else if (mediumCount > 0)
    //            candidates = mediumDistancePlanets;

    //        else if (shortCount > 0)
    //            candidates = shortDistancePlanets;

    //        else
    //        {
    //            candidates = new List<GridTile_Planet>(completeList);
    //            candidates.Remove(startPlanet);
    //        }
    //    }
    //    return candidates;
    //}

    private Client CreateClient(GridTile_Planet startPlanet, GridTile_Planet endPlanet)
    {
        ChallengeType challengeType = _clientRules[_currentRuleIndex].challengeType;

        if (challengeType == ChallengeType.Random)
        {
            int randomRoll = UnityEngine.Random.Range(0, 10);
            if (randomRoll < 5)
                challengeType = ChallengeType.Coords;
            else
                challengeType = ChallengeType.PlanetName;
        }

        if (challengeType == ChallengeType.Coords)
        {
            Client_Coords client = new Client_Coords();
            client.clientFirstName = _contractsDB.GetRandomClientFirstName();
            client.clientLastName = _contractsDB.GetRandomClientLastName();
            client.clientSprite = _contractsDB.GetRandomFaceSprite();
            client.startPlanet = startPlanet;
            client.endPlanet = endPlanet;
            client.maxCompletionTimeInGameMinutes = 0;
            client.challengeType = challengeType;
            return client;
        }
        else
        {
            Client_PlanetName client = new Client_PlanetName();
            client.clientFirstName = _contractsDB.GetRandomClientFirstName();
            client.clientLastName = _contractsDB.GetRandomClientLastName();
            client.clientSprite = _contractsDB.GetRandomFaceSprite();
            client.startPlanet = startPlanet;
            client.endPlanet = endPlanet;
            client.maxCompletionTimeInGameMinutes = 0;
            client.challengeType = challengeType;
            return client;
        }
    }

    private IEnumerator NewContractTimer()
    {
        while (true)
        {
            float interval = _clientRules[_currentRuleIndex].spawnTime;

            if (_allRulesApplied)
                interval = _defaultContractSpawnInterval;

            _spawnRoutine = SpawnContractTimer(interval);
            yield return StartCoroutine(_spawnRoutine);
        }
    }

    private IEnumerator SpawnContractTimer(float spawnInterval)
    {
        float randomOffset = spawnInterval / 8f;
        float randomTime = UnityEngine.Random.Range(spawnInterval - randomOffset, spawnInterval + randomOffset);

        yield return new WaitForSeconds(randomTime);
        CreateNewSingleContract();

        _spawnRoutine = null;
    }


    // ATTENTION!!!! Le spawnTimer index est indépendant du client rule index.
    // Si un contrat double est spawné, le timer index est en ce moment décalé.

    //private int ContractSizeRandomizer()
    //{
    //    int randomRoll = UnityEngine.Random.Range(1, 101);
    //    //Debug.Log("Random roll... " + randomRoll);
    //    if (randomRoll < _doubleContractChance)
    //    {
    //        //Debug.Log("DOUBLE CONTRACT");

    //        _doubleContractChance = 0;
    //        return 2;
    //    }
    //    else
    //    {
    //        //Debug.Log("SINGLE CONTRACT");

    //        _doubleContractChance += 0;
    //        return 1;
    //    }
    //}
}
