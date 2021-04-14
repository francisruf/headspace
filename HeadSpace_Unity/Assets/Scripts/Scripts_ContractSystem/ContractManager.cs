using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractManager : MonoBehaviour
{
    [Header("Spawn settings")]
    public float firstContractSpawnTime;
    public float contractSpawnInterval;

    private ContractsDB _contractsDB;
    private ConveyorBelt belt;
    public GameObject singleContractPrefab;

    

    private int currentContractID;

    private void Awake()
    {
        _contractsDB = GetComponentInChildren<ContractsDB>();
    }

    private void Start()
    {
        belt = FindObjectOfType<ConveyorBelt>();
        currentContractID = 100;
    }

    private void OnEnable()
    {
        GameManager.gameStarted += OnGameStarted;
    }

    private void OnDisable()
    {
        GameManager.gameStarted -= OnGameStarted;
    }

    private void OnGameStarted()
    {
        StartCoroutine(NewContractTimer());
    }

    // TODO : Wrapper ça dans container de contrat de grosseur différente
    public void CreateNewSingleContract()
    {
        Vector2 spawnPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        if (belt != null)
        {
            spawnPos = belt.contractsStartPos.position;
            endPos = belt.contractsEndPos.position;
        }

        MovableContract movableContract = Instantiate(singleContractPrefab).GetComponent<MovableContract>();
        movableContract.transform.position = spawnPos;
        movableContract.InitializeContract(currentContractID, endPos);
        currentContractID++;

        GridTile_Planet startPlanet = null;
        List<Client> allClients = new List<Client>();

        List<GridTile_Planet> planetMatch = PlanetManager.instance.GetRandomPlanetMatch();

        // TODO : Implémenter différents types
        Client_Coords client = new Client_Coords();
        client.clientFirstName = _contractsDB.GetRandomClientFirstName();
        client.clientLastName = _contractsDB.GetRandomClientLastName();
        client.clientSprite = _contractsDB.GetRandomFaceSprite();
        client.startPlanet = planetMatch[0];
        client.endPlanet = planetMatch[1];
        client.maxCompletionTimeInGameMinutes = 0;

        allClients.Add(client);

        Contract_Single contract = movableContract.GetComponent<Contract_Single>();
        contract.AssignClients(allClients);
    }

    private IEnumerator NewContractTimer()
    {
        yield return new WaitForSeconds(firstContractSpawnTime);
        CreateNewSingleContract();

        while (true)
        {
            yield return new WaitForSeconds(contractSpawnInterval);
            StartCoroutine(SpawnContractTimer(contractSpawnInterval));
        }
    }

    private IEnumerator SpawnContractTimer(float spawnInterval)
    {
        float halfTime = spawnInterval / 4f;
        float randomTime = Random.Range(0f, halfTime);

        yield return new WaitForSeconds(randomTime);
        CreateNewSingleContract();
    }
}
