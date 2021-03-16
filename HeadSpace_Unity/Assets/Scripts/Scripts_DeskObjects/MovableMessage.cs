using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovableMessage : MovableObject
{
    public Action messageTeared;

    private TextMeshProUGUI _messageText;
    private Canvas _canvas;
    private bool _wasTeared;

    // Disable le collider du message (ne peut être sélectionné qu'une fois complètement imprimé)
    protected override void Awake()
    {
        base.Awake();
        _messageText = GetComponentInChildren<TextMeshProUGUI>();
        _canvas = GetComponentInChildren<Canvas>();
        _collider.enabled = false;
    }

    public override void Select()
    {
        // Appeler l'action messageTeared, la première fois que l'objet est Selected
        if (!_wasTeared)
        {
            _wasTeared = true;

            if (messageTeared != null)
                messageTeared();
        }
        base.Select();
    }

    public void EnableCollider()
    {
        _collider.enabled = true;
    }

    public void SetText(string messageText)
    {
        _messageText.text = messageText;
    }

    public override void DisableObject()
    {
        base.DisableObject();
        _messageText.enabled = false;
        _canvas.enabled = false;
    }
}
