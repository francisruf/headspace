using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjectInteractionZone : MonoBehaviour
{
    public Action<ObjectInteractionZone> interactRequest;
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;

    private bool _isEnabled;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool IsInBounds(Vector2 targetPoint)
    {
        if (!_isEnabled)
            return false;

        return _collider.bounds.Contains(targetPoint);
    }

    public void Interact()
    {
        if (interactRequest != null)
            interactRequest(this);
    }

    public void ToggleZone(bool toggleON)
    {
        if (_collider != null)
            _collider.enabled = toggleON;

        if (_spriteRenderer != null)
            _spriteRenderer.enabled = toggleON;

        _isEnabled = toggleON;
    }
}
