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
        GridTile_Planet startPlanet = null;

        if (_clientRules[_currentRuleIndex].specialCondition == SpecialConditions.ClosestPlanetToShip)
        {
            startPlanet = GetClosestPlanet(allCandidatePlanets);
        }
        else
        {
            List<GridTile_Planet> candidateStartPlanets = GetStartPlanets(allCandidatePlanets);
            startPlanet = ExtractRandomPlanet(candidateStartPlanets);
        }

        if (startPlanet == null)
        {
            Debug.LogWarning("Could not find target start planet. Check start settings.");
            int randomIndex = UnityEngine.Random.Range(0, allCandidatePlanets.Count);
            startPlanet = allCandidatePlanets[randomIndex];
        }

        for (int i = 0; i < contractSize; i++)
        {
            // End planet
            List<GridTile_Planet> candidateEndPlanets = GetEndPlanets(startPlanet, allCandidatePlanets);
            List<GridTile_Planet> distanceCandidates = GetPlanetsByDistance(allCandidatePlanets, candidateEndPlanets, startPlanet);
            GridTile_Planet endPlanet = ExtractRandomPlanet(distanceCandidates);

            allClients.Add(CreateClient(startPlanet, endPlanet));
            allCandidatePlanets.Remove(endPlanet);
            int ruleCount = _clientRules.Count;

            if (_currentRuleIndex == _clientRules.Count - 1)
                _allRulesApplied = true;

            if (!_allRulesApplied)
                _currentRuleIndex++;
            else
                _currentRuleIndex = UnityEngine.Random.Range(0, ruleCount);
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

    private GridTile_Planet ExtractRandomPlanet(List<GridTile_Planet> originalList)
    {
        int randomIndex = UnityEngine.Random.Range(0, originalList.Count);
        GridTile_Planet target = originalList[randomIndex];
        originalList.Remove(target);
        return target;
    }

    private GridTile_Planet GetClosestPlanet(List<GridTile_Planet> completeList)
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
                int distanceRating = PathFinder.instance.GetDistanceRating(shipPos.tileX, shipPos.tileY, planetPos.tileX, planetPos.tileY);
                if (distanceRating < minRating)
                {
                    minRating = distanceRating;
                    targetIndex = i;
                }
            }
        }

        if (targetIndex < count)
        {
            return completeList[targetIndex];
        }

        return null;
    }

    private List<GridTile_Planet> GetStartPlanets(List<GridTile_Planet> completeList)
    {
        //StartPlanetState startState = _clientRules[_currentRuleIndex].startPlanetState;
        int count = 0;
        List<GridTile_Planet> candidatePlanets = new List<GridTile_Planet>();

        foreach (var planet in completeList)
        {
            candidatePlanets.Add(planet);
            count++;
        }

        //if (startState == StartPlanetState.Random)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        candidatePlanets.Add(planet);
        //    }
        //}
        //else if (startState == StartPlanetState.Found)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        if (planet.PlanetFound)
        //        {
        //            candidatePlanets.Add(planet);
        //            count++;
        //        }
        //    }
        //}
        //else if (startState == StartPlanetState.NotFound)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        if (!planet.PlanetFound)
        //        {
        //            candidatePlanets.Add(planet);
        //            count++;
        //        }
        //    }
        //}

        if (count > 0)
            return candidatePlanets;
        else
            return new List<GridTile_Planet>(completeList);
    }

    private List<GridTile_Planet> GetEndPlanets(GridTile_Planet startPlanet, List<GridTile_Planet> completeList)
    {
        //EndPlanetState endState = _clientRules[_currentRuleIndex].endPlanetState;
        int count = 0;
        List<GridTile_Planet> candidatePlanets = new List<GridTile_Planet>();

        foreach (var planet in completeList)
        {
            if (planet != startPlanet)
            {
                candidatePlanets.Add(planet);
                count++;
            }
                
        }

        //if (endState == EndPlanetState.Random)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        if (planet != startPlanet)
        //            candidatePlanets.Add(planet);
        //    }
        //}
        //else if (endState == EndPlanetState.Found)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        if (planet != startPlanet)
        //            if (planet.PlanetFound)
        //            {
        //                candidatePlanets.Add(planet);
        //                count++;
        //            }
        //    }
        //}
        //else if (endState == EndPlanetState.NotFound)
        //{
        //    foreach (var planet in completeList)
        //    {
        //        if (planet != startPlanet)
        //            if (!planet.PlanetFound)
        //            {
        //                candidatePlanets.Add(planet);
        //                count++;
        //            }
        //    }
        //}

        if (count > 0)
            return candidatePlanets;
        else
        {
            candidatePlanets = new List<GridTile_Planet>(completeList);
            candidatePlanets.Remove(startPlanet);
            return candidatePlanets;
        }
    }

    private List<GridTile_Planet> GetPlanetsByDistance(List<GridTile_Planet> completeList, List<GridTile_Planet> listToModify, GridTile_Planet startPlanet)
    {
        int targetDistanceRating = _clientRules[_currentRuleIndex].distanceRating;

        List<GridTile_Planet> shortDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> mediumDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> longDistancePlanets = new List<GridTile_Planet>();
        List<GridTile_Planet> candidates = new List<GridTile_Planet>();
        int shortCount = 0;
        int mediumCount = 0;
        int longCount = 0;

        List<int> distances = new List<int>();
        int count = 0;
        foreach (var planet in completeList)
        {
            distances.Add(planet.DistanceRating);
            count++;
        }
        distances.Sort();

        int third = distances[(count / 3) - 1];
        int twoThirds = distances[(count / 3 * 2) - 1];

        foreach (var planet in listToModify)
        {
            if (planet.DistanceRating <= third)
            {
                shortDistancePlanets.Add(planet);
                shortCount++;
            }
            else if (planet.DistanceRating > twoThirds)
            {
                longDistancePlanets.Add(planet);
                longCount++;
            }
            else
            {
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
