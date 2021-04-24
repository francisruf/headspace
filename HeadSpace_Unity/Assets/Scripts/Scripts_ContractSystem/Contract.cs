using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Contract : MonoBehaviour
{
    public static Action<int> contractComplete;

    [Header("Client settings")]
    public int targetClientCount;
    protected int currentClientCount;
    protected int pointsReward;
    protected bool _complete;

    [Header("UI references")]
    public SpriteRenderer startPlanetSpriteRenderer;
    public TextMeshProUGUI startPlanetText;
    public TextMeshProUGUI rewardsText;

    [Header("Client components")]
    public List<TextMeshProUGUI> clientNameTexts;
    public List<SpriteRenderer> clientFaceRenderers;
    public List<TextMeshProUGUI> clientDestinationTexts;
    public List<SpriteRenderer> clientChipRenderers;

    [Header("Chip states sprites")]
    public Sprite chipWaitingSprite;
    public Sprite chipEmbarkedSprite;
    public Sprite chipDebarkedSprite;
    public Sprite chipDeadSprite;

    protected List<Client> _allClients = new List<Client>();
    public List<Client> AllClients { get { return _allClients; } }

    public void AssignClients(List<Client> clients)
    {
        if (clients[0].startPlanet != null)
        {
            startPlanetSpriteRenderer.sprite = clients[0].startPlanet.SpriteMatch.contractSprite;
            startPlanetText.text = clients[0].startPlanet.PlanetName;
        }
        else
            startPlanetText.text = "Some planet";

        for (int i = 0; i < clients.Count; i++)
        {
            if (i >= targetClientCount)
                break;

            AddClient(clients[i], i);
        }
    }

    protected virtual void AddClient(Client client, int index)
    {
        _allClients.Add(client);

        clientNameTexts[index].text = client.GetDisplayName();
        clientFaceRenderers[index].sprite = client.clientSprite;
        clientDestinationTexts[index].text = client.GetDestinationInfo();

        client.clientStateChanged += OnClientStateChanged;
    }

    private void OnClientStateChanged(Client client)
    {
        int index = _allClients.IndexOf(client);
        Sprite targetSprite = null;

        switch (client.currentState)
        {
            case ClientState.Waiting:
                targetSprite = chipWaitingSprite;
                break;

            case ClientState.Embarked:
                targetSprite = chipEmbarkedSprite;
                break;

            case ClientState.Debarked:
                targetSprite = chipDebarkedSprite;
                CheckCompletion();
                break;

            case ClientState.Dead:
                targetSprite = chipDeadSprite;
                break;

            default:
                break;
        }
        clientChipRenderers[index].sprite = targetSprite;
    }

    private void CheckCompletion()
    {
        if (_complete)
            return;

        foreach (var client in _allClients)
        {
            if (client.currentState != ClientState.Debarked)
                return;
        }
        Debug.Log("CONTRACT COMPLETE! HURRAY!");
        if (contractComplete != null)
            contractComplete(pointsReward);

        _complete = true;
    }

    public void CalculatePointsReward(ContractPointsSettings settings)
    {
        foreach (var client in _allClients)
        {
            pointsReward += GetChallengePoints(settings, client);
            //Debug.Log("Challenge points : " + GetChallengePoints(settings, client));
            pointsReward += GetStartPlanetPoints(settings, client);
            //Debug.Log("Start planet points : " + GetStartPlanetPoints(settings, client));
            pointsReward += GetEndPlanetPoints(settings, client);
            //Debug.Log("End planet points : " + GetEndPlanetPoints(settings, client));
            pointsReward += GetDistanceTravelledPoints(settings, client);
            //Debug.Log("DistanceTravelled points : " + GetDistanceTravelledPoints(settings, client));
        }
        rewardsText.text = pointsReward + "c";
    }

    private int GetChallengePoints(ContractPointsSettings settings, Client client)
    {
        if (client.challengeType == ChallengeType.Coords)
            return settings.CoordsChallengePoints;

        else if (client.challengeType == ChallengeType.PlanetName)
            return settings.PlanetNameChallengePoints;

        return 0;
    }

    private int GetStartPlanetPoints(ContractPointsSettings settings, Client client)
    {
        List<GridTile_Planet> allPlanets = PlanetManager.instance.AllPlanetTiles;
        List<int> distances = new List<int>();
        int count = 0;
        foreach (var planet in allPlanets)
        {
            distances.Add(planet.DistanceRating);
            count++;
        }
        distances.Sort();

        int index = distances.IndexOf(client.startPlanet.DistanceRating);

        int third = (count / 3) + 1;
        int twoThirds = ((count / 3) * 2) + 1;

        if (index <= third)
            return settings.startShortDistancePoints;
        else if (index > twoThirds)
            return settings.startLongDistancePoints;
        else
            return settings.startMediumDistancePoints;
    }

    private int GetEndPlanetPoints(ContractPointsSettings settings, Client client)
    {
        if (client.endPlanet != null)
        {
            List<GridTile_Planet> allPlanets = PlanetManager.instance.AllPlanetTiles;
            List<int> distances = new List<int>();
            int count = 0;
            foreach (var planet in allPlanets)
            {
                distances.Add(planet.DistanceRating);
                count++;
            }
            distances.Sort();

            int index = distances.IndexOf(client.endPlanet.DistanceRating);

            int third = (count / 3) + 1;
            int twoThirds = ((count / 3) * 2) + 1;

            if (index <= third)
                return settings.startShortDistancePoints;
            else if (index > twoThirds)
                return settings.startLongDistancePoints;
        }
        return settings.startMediumDistancePoints;
    }

    private int GetDistanceTravelledPoints(ContractPointsSettings settings, Client client)
    {
        int nodeCount = 0;

        if (client.endPlanet != null)
        {
            List<PathNode> pathToTravel = PathFinder.instance.FindPath(
                client.endPlanet.tileX, client.endPlanet.tileY, client.startPlanet.tileX, client.startPlanet.tileY);

            if (pathToTravel != null)
                nodeCount = pathToTravel.Count;
        }
        else
        {
            nodeCount = Mathf.RoundToInt(GridCoords.CurrentGridInfo.gameGridSize.x / 1.5f);
        }

        return Mathf.RoundToInt(nodeCount * settings.pointsPerTile);
    }
}
