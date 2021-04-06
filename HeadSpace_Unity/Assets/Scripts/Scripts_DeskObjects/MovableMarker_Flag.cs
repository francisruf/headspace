using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovableMarker_Flag : MovableMarker
{
    private TextMeshProUGUI _tileText;

    protected override void Awake()
    {
        base.Awake();
        _tileText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void Select()
    {
        base.Select();
        _tileText.enabled = false;
    }

    public override void Deselect()
    {
        base.Deselect();
    }

    protected override void CheckIfInTile()
    {
        base.CheckIfInTile();

        if (_isInTile)
        {
            _tileText.text = GridCoords.GetTileName(_currentTile);
            _tileText.enabled = true;
        }
        else
        {
            _tileText.enabled = false;
        }
    }
}
