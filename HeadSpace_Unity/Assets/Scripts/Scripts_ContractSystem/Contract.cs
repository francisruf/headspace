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
    public int PointsReward { get { return pointsReward; } }
    protected int _travelDistanceRating;

    protected bool _complete;
    public bool IsComplete { get { return _complete; } }
    protected bool _failed;
    public bool IsFailed { get { return _failed; } }
    protected int _clientsCompleted;
    public int ClientsCompleted { get { return _clientsCompleted; } }
    protected GridTile_Planet _startPlanet;
    public GridTile_Planet StartPlanet { get { return _startPlanet; } }

    [Header("Time settings")]
    public Color chillColor;
    public Color hurryColor;
    public Color dangerColor;
    public Color failColor;
    private int _completionTime;
    private int _currentTime;
    private bool _timedContract;
    private IEnumerator _currentTimer;
    private IEnumerator _currentSliderfail;

    [Header("UI references")]
    public SpriteRenderer startPlanetSpriteRenderer;
    public TextMeshProUGUI startPlanetText;
    public TextMeshProUGUI rewardsText;
    public SpriteRenderer timeSlider;

    [Header("Client components")]
    public List<TextMeshProUGUI> clientNameTexts;
    public List<SpriteRenderer> clientFaceRenderers;
    public List<TextMeshProUGUI> clientDestinationTexts;
    public List<SpriteRenderer> clientStartStatusRenderers;
    public List<SpriteRenderer> clientEndStatusRenderers;
    public List<SpriteRenderer> destinationPlanetRenderers;

    [Header("Chip states sprites")]
    public Sprite statusCompletedSprite;
    public Sprite statusFailedSprite;
    public Sprite deadFaceSprite;

    protected List<Client> _allClients = new List<Client>();
    public List<Client> AllClients { get { return _allClients; } }

    public void AssignClients(List<Client> clients, int completionTime, bool timedContract)
    {
        if (clients[0].startPlanet != null)
        {
            startPlanetSpriteRenderer.sprite = clients[0].startPlanet.SpriteMatch.contractSprite;
            startPlanetText.text = clients[0].startPlanet.PlanetName;
            _startPlanet = clients[0].startPlanet;
        }
        else
            startPlanetText.text = "Some planet";

        int candidateDistance = 0;
        for (int i = 0; i < clients.Count; i++)
        {
            if (i >= targetClientCount)
                break;

            if (clients[i].travelDistanceRating > candidateDistance)
                candidateDistance = clients[i].travelDistanceRating;

            AddClient(clients[i], i);
        }

        _travelDistanceRating = candidateDistance;
        this._timedContract = timedContract;
        this._completionTime = completionTime + (_travelDistanceRating * 60);
        _currentTime = 0;

        timeSlider.color = chillColor;
        timeSlider.enabled = _timedContract;

        Debug.Log("Client " + clients[0].GetDisplayName() + " - rating : " + _travelDistanceRating + " - timer : " + _completionTime);
    }

    protected virtual void AddClient(Client client, int index)
    {
        clientStartStatusRenderers[index].sprite = null;
        clientEndStatusRenderers[index].sprite = null;

        _allClients.Add(client);

        clientNameTexts[index].text = client.GetDisplayName();
        clientFaceRenderers[index].sprite = client.clientSprite;
        clientDestinationTexts[index].text = client.GetDestinationInfo();

        if (client.endPlanet != null)
            destinationPlanetRenderers[index].sprite = client.endPlanet.SpriteMatch.contractSprite;

        client.clientStateChanged += OnClientStateChanged;
    }

    private void ContractFailed(bool clientDead)
    {
        bool started = false;
        foreach (var client in _allClients)
        {
            if (client.currentState == ClientState.Embarked)
                started = true;
            else if (client.currentState != ClientState.Dead)
                client.ChangeState(ClientState.Left);
        }

        if (!started || clientDead)
        {
            if (_currentTimer != null)
            {
                StopCoroutine(_currentTimer);
                _currentTimer = null;
            }

            foreach (var clientStartStatus in clientStartStatusRenderers)
            {
                clientStartStatus.sprite = statusFailedSprite;
            }
            foreach (var clientEndStatus in clientEndStatusRenderers)
            {
                clientEndStatus.sprite = statusFailedSprite;
            }
            timeSlider.enabled = true;
            Vector2 size = timeSlider.size;
            size.x = 0.14f;
            timeSlider.size = size;
            timeSlider.color = failColor;
            _complete = true;
        }

        else
        {
            _currentSliderfail = SliderFailState();
            StartCoroutine(_currentSliderfail);
        }

        _failed = true;
    }

    private void OnClientStateChanged(Client client)
    {
        int index = _allClients.IndexOf(client);

        switch (client.currentState)
        {
            case ClientState.Waiting:
                clientStartStatusRenderers[index].sprite = null;
                clientEndStatusRenderers[index].sprite = null;
                break;

            case ClientState.Embarked:
                clientStartStatusRenderers[index].sprite = statusCompletedSprite;
                break;

            case ClientState.Debarked:
                clientStartStatusRenderers[index].sprite = statusCompletedSprite;
                clientEndStatusRenderers[index].sprite = statusCompletedSprite;
                _clientsCompleted++;
                CheckCompletion();
                break;

            case ClientState.Left:
                clientStartStatusRenderers[index].sprite = statusFailedSprite;
                clientEndStatusRenderers[index].sprite = statusFailedSprite;
                break;

            case ClientState.Dead:
                clientStartStatusRenderers[index].sprite = statusFailedSprite;
                clientEndStatusRenderers[index].sprite = statusFailedSprite;
                clientFaceRenderers[index].sprite = deadFaceSprite;
                clientNameTexts[index].color = failColor;
                ContractFailed(true);
                break;

            default:
                break;
        }
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

        if (_currentTimer != null)
        {
            StopCoroutine(_currentTimer);
            _currentTimer = null;
        }

        if (_currentSliderfail != null)
        {
            StopCoroutine(_currentSliderfail);
            _currentSliderfail = null;

            timeSlider.enabled = true;
        }

        if (contractComplete != null)
            contractComplete(GetFinalPoints());

        _complete = true;
    }

    public int GetFinalPoints()
    {
        if (_clientsCompleted <= 0)
            return 0;

        int points = 10;

        if (_timedContract)
        {
            if (_currentTime < _completionTime / 3f)
            {
                points = 10;
            }
            else if (_currentTime > _completionTime * 0.6666f && _currentTime < _completionTime)
            {
                points = 6;
            }
            else
            {
                points = 8;
            }
        }

        if (_failed)
        {
            if (_complete)
                points = 1;
            else
                points = 0;
        }

        return points;
    }

    public void CalculatePointsReward(ContractPointsSettings settings)
    {
        foreach (var client in _allClients)
        {
            pointsReward += 10;

            //pointsReward += GetChallengePoints(settings, client);
            //Debug.Log("Challenge points : " + GetChallengePoints(settings, client));
            //pointsReward += GetStartPlanetPoints(settings, client);
            //Debug.Log("Start planet points : " + GetStartPlanetPoints(settings, client));
            //pointsReward += GetEndPlanetPoints(settings, client);
            //Debug.Log("End planet points : " + GetEndPlanetPoints(settings, client));
            //pointsReward += GetDistanceTravelledPoints(settings, client);
            //Debug.Log("DistanceTravelled points : " + GetDistanceTravelledPoints(settings, client));
        }
        rewardsText.text = pointsReward + "M";
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
        //List<GridTile_Planet> allPlanets = PlanetManager.instance.AllPlanetTiles;
        //List<int> distances = new List<int>();
        //int count = 0;
        //foreach (var planet in allPlanets)
        //{
        //    distances.Add(planet.DistanceRating);
        //    count++;
        //}
        //distances.Sort();

        //int index = distances.IndexOf(client.startPlanet.DistanceRating);

        //int third = (count / 3) + 1;
        //int twoThirds = ((count / 3) * 2) + 1;

        //if (index <= third)
        //    return settings.startShortDistancePoints;
        //else if (index > twoThirds)
        //    return settings.startLongDistancePoints;
        //else
        //    return settings.startMediumDistancePoints;
        return 0;
    }

    private int GetEndPlanetPoints(ContractPointsSettings settings, Client client)
    {
        //if (client.endPlanet != null)
        //{
        //    List<GridTile_Planet> allPlanets = PlanetManager.instance.AllPlanetTiles;
        //    List<int> distances = new List<int>();
        //    int count = 0;
        //    foreach (var planet in allPlanets)
        //    {
        //        distances.Add(planet.DistanceRating);
        //        count++;
        //    }
        //    distances.Sort();

        //    int index = distances.IndexOf(client.endPlanet.DistanceRating);

        //    int third = (count / 3) + 1;
        //    int twoThirds = ((count / 3) * 2) + 1;

        //    if (index <= third)
        //        return settings.startShortDistancePoints;
        //    else if (index > twoThirds)
        //        return settings.startLongDistancePoints;
        //}
        //return settings.startMediumDistancePoints;

        return 0;
    }

    private int GetDistanceTravelledPoints(ContractPointsSettings settings, Client client)
    {
        int nodeCount = 0;

        if (client.endPlanet != null)
        {
            List<PathNode> pathToTravel = PathFinder.instance.FindPath(
                client.endPlanet.tileX, client.endPlanet.tileY, client.startPlanet.tileX, client.startPlanet.tileY, true);

            if (pathToTravel != null)
                nodeCount = pathToTravel.Count;
        }
        else
        {
            nodeCount = Mathf.RoundToInt(GridCoords.CurrentGridInfo.gameGridSize.x / 1.5f);
        }

        return Mathf.RoundToInt(nodeCount * settings.pointsPerTile);
    }

    public void OnContractBeltExit()
    {
        if (_timedContract)
        {
            _currentTimer = ContractTimer();
            StartCoroutine(_currentTimer);
        }
    }

    private IEnumerator ContractTimer()
    {
        while (_currentTime < _completionTime)
        {
            yield return new WaitForSeconds(1f);
            _currentTime += 1;
            UpdateSlider();
        }
        ContractFailed(false);
        _currentTimer = null;
    }

    private IEnumerator SliderFailState()
    {
        Vector2 size = timeSlider.size;
        size.x = 0.14f;
        timeSlider.size = size;
        timeSlider.color = failColor;

        while (true)
        {
            timeSlider.enabled = true;
            yield return new WaitForSeconds(2f);
            timeSlider.enabled = false;
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateSlider()
    {
        float width = 0.14f - ((float)_currentTime / _completionTime * 0.14f);
        float roundedWidth = (Mathf.CeilToInt(width * 100)) / 100f;
        Debug.Log("Rounded witdth " + roundedWidth);
        
        Vector2 size = timeSlider.size;
        size.x = roundedWidth;
        timeSlider.size = size;

        if (_currentTime < _completionTime / 3f)
        {
            timeSlider.color = chillColor;
        }
        else if (_currentTime > _completionTime * 0.6666f)
        {
            timeSlider.color = dangerColor;
        }
        else
        {
            timeSlider.color = hurryColor;
        }
    }
}
