using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Contract : MonoBehaviour
{
    [Header("Client settings")]
    public int targetClientCount;
    protected int currentClientCount;
    protected int pointsReward;

    [Header("UI references")]
    public SpriteRenderer startPlanetSpriteRenderer;
    public TextMeshProUGUI startPlanetText;

    protected List<Client> _allClients = new List<Client>();

    public void AssignClients(List<Client> clients)
    {
        if (clients[0].startPlanet != null)
        {
            startPlanetSpriteRenderer.sprite = clients[0].startPlanet.SpriteMatch.contractSprite;
            startPlanetText.text = clients[0].startPlanet.PlanetName;
        }
        else
            startPlanetText.text = "Some planet";

        foreach (var cl in clients)
        {
            if (currentClientCount < targetClientCount)
            {
                AddClient(cl);
                targetClientCount++;
            }
        }
    }

    protected virtual void AddClient(Client client)
    {
        _allClients.Add(client);
    }

    public bool CheckCompletion()
    {
        foreach (var client in _allClients)
        {
            if (client.currentState != ClientState.Arrived)
                return false;
        }
        return true;
    }
}
