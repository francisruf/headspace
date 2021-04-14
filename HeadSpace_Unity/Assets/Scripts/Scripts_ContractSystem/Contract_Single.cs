using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Contract_Single : Contract
{
    [Header("Client 0")]
    public TextMeshProUGUI client0NameText;
    public SpriteRenderer client0SpriteRenderer;
    public TextMeshProUGUI client0DestinationText;
    //public SpriteRenderer client0DesintationIcon0;

    protected override void AddClient(Client client)
    {
        
        base.AddClient(client);
        client0NameText.text = client.GetDisplayName();
        Debug.Log("hello");
        client0SpriteRenderer.sprite = client.clientSprite;
        client0DestinationText.text = client.GetDestinationInfo();
    }
}
