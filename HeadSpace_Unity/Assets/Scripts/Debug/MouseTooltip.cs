using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MouseTooltip : MonoBehaviour
{
    // Variables
    private GridInfo _currentGridInfo;  // Stock les informations de la grille actuelle

    // Texte UI
    private TextMeshProUGUI _mouseCoordsText;

    // Layers pour le raycast
    public LayerMask planetLayerMask;

    // States
    private bool _mouseOverGrid;
    private bool _mouseOverPlanet;
    private string _coordsText;
    private string _planetText;

    // Subscription à l'action newGameGrid
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
        DebugManager.mouseToolTipActiveState += ToggleToolTip;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
        DebugManager.mouseToolTipActiveState -= ToggleToolTip;
    }

    private void Awake()
    {
        _mouseCoordsText = GetComponentInChildren<TextMeshProUGUI>();
        _mouseCoordsText.enabled = false;
    }

    private void Start()
    {
        ToggleToolTip(false);
    }

    private void ToggleToolTip(bool ToggleON)
    {
        _mouseCoordsText.gameObject.SetActive(ToggleON);
    }

    // Assigne les valeurs de _currentGridInfo (appel à chaque nouvelle grille)
    private void AssignGridInfo(GridInfo info)
    {
        _currentGridInfo = info;
    }

    private void Update()
    {
        // Si gridInfo n'a pas été assigné encore, arrêter l'exécution
        if (_currentGridInfo == null)
            return;

        Vector3 mousePos = Vector3.zero;
        _mouseOverGrid = MouseIsOverGrid(out mousePos);
        _mouseOverPlanet = MouseIsOverPlanet();
        
        if (_mouseOverPlanet)
        {
            UpdateTooltip(mousePos, _planetText);
        }
        else if (_mouseOverGrid)
        {
            UpdateTooltip(mousePos, _coordsText);
        }
        else
        {
            if (_mouseCoordsText.enabled)
                _mouseCoordsText.enabled = false;
        }
    }

    // Vérifier si la souris se trouve sur la grille
    private bool MouseIsOverGrid(out Vector3 mousePos)
    {
        // Convertir position de souris en pixels en une position en WORLD COORDS
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;
        mousePos = mousePosition;

        // Assigner le texte
        _coordsText = GridCoords.FromWorldToGrid(mousePos).ToString();

        // Vérifier si la position se trouve dans les bounds de la grille.
        if (_currentGridInfo.gameGridWorldBounds.Contains(mousePosition))
        {
            return true;
        }
        return false;
    }

    // Vérifier si la souris se trouve par dessus une planète
    private bool MouseIsOverPlanet()
    {
        // Raycast au curseur
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, planetLayerMask);

        // Si un collider
        foreach (var hitInfo in hitsInfo)
        {
            if (hitInfo.collider != null)
            {
                // Si un collider de Planet (zone d'interaction) est trouvé (VRAI)
                Planet_InteractionZone planetZone = hitInfo.collider.GetComponent<Planet_InteractionZone>();
                if (planetZone != null)
                {
                    // Aller chercher le script planet
                    Planet planet = planetZone.GetComponentInParent<Planet>();
                    // Assigner les propriétés de texte
                    _planetText = "Souls : " + planet.CurrentSouls + "/" + planet.TotalSouls;
                    _planetText += "\nCoords : " + planet.GridCoordinates;
                    _planetText += "\nTile : (" + planet.ParentTile.tileX + ", " + planet.ParentTile.tileY + ")";

                    return true;
                }
            }
        }
        // Sinon, retourner FAUX
        return false;
    }

    private void UpdateTooltip(Vector3 mousePos, string debugText)
    {
        // Activer le texte si pas encore actif
        if (!_mouseCoordsText.enabled)
            _mouseCoordsText.enabled = true;

        Vector3 tooltipPosition = Vector3.zero;
        tooltipPosition.x = Input.mousePosition.x + 20f;
        tooltipPosition.y = Input.mousePosition.y;

        _mouseCoordsText.transform.position = tooltipPosition;
        _mouseCoordsText.text = debugText;
    }
}
