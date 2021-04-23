using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandChar : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private CharMatch _match;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(CharMatch match)
    {
        this._match = match;
        _spriteRenderer.sprite = _match.offSprite;
    }

    public void ToggleChar(bool toggleON)
    {
        if (toggleON)
            _spriteRenderer.sprite = _match.onSprite;
        else
            _spriteRenderer.sprite = _match.offSprite;
    }
}
