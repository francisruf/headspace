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

    [Header("Client components")]
    public List<TextMeshProUGUI> clientNameTexts;
    public List<SpriteRenderer> clientFaceRenderers;
    public List<TextMeshProUGUI> clientDestinationTexts;

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
        Debug.Log("hello");
        clientFaceRenderers[index].sprite = client.clientSprite;
        clientDestinationTexts[index].text = client.GetDestinationInfo();
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
