using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipMarker : MovableMarker
{
    public static Action<ShipMarker> shipMarkerReceived;

    // Références aux components
    private TextMeshProUGUI _markerText;
    private MovableMarker _movableMarker;
    private ShipMarker_Animator _markerAnimator;

    // Variables
    private Ship _linkedShip;
    private string _shipCallsign;

    protected override void Awake()
    {
        base.Awake();

        // Assigner la référence aux components
        _markerText = GetComponentInChildren<TextMeshProUGUI>();
        _movableMarker = GetComponent<MovableMarker>();
        _markerAnimator = GetComponentInParent<ShipMarker_Animator>();
    }

    // Subscribe aux changements d'informations des Ships
    private void OnEnable()
    {
        Ship.shipInfoChange += OnShipInfoChange;
    }

    // Unsubscribe
    private void OnDisable()
    {
        Ship.shipInfoChange -= OnShipInfoChange;
    }

    // Fonction qui assigne les paramètres initiaux, appelée à partir du vaisseau
    public void InitializeMarker(Ship linkedShip)
    {
        this._linkedShip = linkedShip;
        this._shipCallsign = linkedShip.shipCallsign;
        UpdateMarkerText();

        if (placeObjectRequest != null)
            placeObjectRequest(this, ObjectSpawnZone.Outbox);

        if (shipMarkerReceived != null)
            shipMarkerReceived(this);
    }

    // TODO
    public override void DisableObject()
    {
        base.DisableObject();

        _markerText.enabled = false;
        _movableMarker.enabled = false;
        _linkedShip.enabled = false;
    }

    // TODO
    public void DestroyMarker()
    {
        _movableMarker.enabled = false;
        _collider.enabled = false;
        _linkedShip = null;

        _markerAnimator.TriggerDestroy();
    }

    public void OnDestroyAnimationComplete()
    {
        _spriteRenderer.enabled = false;
        _movableMarker.enabled = false;
    }

    // Action appelée à chaque changement d'information de Ship
    private void OnShipInfoChange(Ship ship)
    {
        // S'il ne s'agit pas du ship lié, ne rien faire
        if (ship != _linkedShip)
            return;

        // Sinon
        UpdateMarkerText();
    }

    // Fonction générale de mise à jour du texte
    private void UpdateMarkerText()
    {
        _markerText.text = _linkedShip.shipCallsign.ToUpper();
    }
}
