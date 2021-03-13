using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MouseCoordsToolTip : MonoBehaviour
{
    // Variables
    private GridInfo _currentGridInfo;  // Stock les informations de la grille actuelle

    // Texte UI
    private TextMeshProUGUI _mouseCoordsText;

    // Layers pour le raycast
    public LayerMask objectsLayerMask;

    // States
    private bool _mouseOverGrid;
    private bool _mouseOverMarker;
    private bool _markerSelected;
    private bool _objectInFront;
    private string _coordsText;

    // Position du tooltip
    private Vector3 _currentTooltipPos = Vector3.zero;

    // Objet sélectionné
    private MovableObject _selectedObject;

    // Subscription à plusieurs actions
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
        DebugManager.mouseToolTipActiveState += ToggleToolTip;
        MovableObject.movableObjectSelected += OnMovableObjectSelected;
        MovableObject.movableObjectDeselected += OnMovableObjectDeselected;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
        DebugManager.mouseToolTipActiveState -= ToggleToolTip;
        MovableObject.movableObjectSelected -= OnMovableObjectSelected;
        MovableObject.movableObjectDeselected -= OnMovableObjectDeselected;
    }

    private void Awake()
    {
        _mouseCoordsText = GetComponentInChildren<TextMeshProUGUI>();
        _mouseCoordsText.enabled = false;
    }

    private void Start()
    {
        _mouseCoordsText.gameObject.SetActive(true);
    }

    private void ToggleToolTip(bool debugTooltipIsActive)
    {
        _mouseCoordsText.gameObject.SetActive(!debugTooltipIsActive);
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

        _mouseOverGrid = MouseIsOverGrid();

        if (_mouseOverGrid)
        {
            if (MarkerIsSelected())
            {
                UpdateTooltip(_currentTooltipPos, _coordsText);
            }
            else if (MouseIsOverMarker())
            {
                UpdateTooltip(_currentTooltipPos, _coordsText);
            }
            else
            {
                _currentTooltipPos = Vector3.zero;
                _currentTooltipPos.x = Input.mousePosition.x + 20f;
                _currentTooltipPos.y = Input.mousePosition.y - 10f;

                UpdateTooltip(_currentTooltipPos, _coordsText);
            }
        }
        else
        {
            if (_mouseCoordsText.enabled)
                _mouseCoordsText.enabled = false;
        }
    }

    // Vérifier si la souris se trouve sur la grille
    private bool MouseIsOverGrid()
    {
        // Convertir position de souris en pixels en une position en WORLD COORDS
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Vérifier si la position se trouve dans les bounds de la grille.
        if (_currentGridInfo.gameGridWorldBounds.Contains(mousePosition))
        {
            // Assigner le texte
            _coordsText = GridCoords.FromWorldToGrid(mousePosition).ToString();

            return true;
        }
        return false;
    }

    // Vérifier si la souris se trouve par dessus une planète
    private bool MouseIsOverMarker()
    {
        _objectInFront = false;

        // Raycast au curseur
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, objectsLayerMask);
        int hitCount = hitsInfo.Length;
        SpriteRenderer topSpriteRenderer = null;

        // Si un trouve un/des collider(s)
        for (int i = 0; i < hitCount; i++)
        {
            // Calculer quel objet est renderer prioritairement et assigner la position sur celui-ci
            SpriteRenderer candidate = hitsInfo[i].collider.GetComponent<SpriteRenderer>();
            if (candidate != null)
            {
                if (topSpriteRenderer == null)
                {
                    topSpriteRenderer = candidate;
                }
                else
                {
                    int candidateLayer = SortingLayer.GetLayerValueFromID(candidate.sortingLayerID);
                    int topRendererLayer = SortingLayer.GetLayerValueFromID(topSpriteRenderer.sortingLayerID);

                    if (candidateLayer > topRendererLayer)
                    {
                        topSpriteRenderer = candidate;
                    }
                    else if (candidateLayer == topRendererLayer)
                    {
                        if (candidate.sortingOrder > topSpriteRenderer.sortingOrder)
                        {
                            topSpriteRenderer = candidate;
                        }
                    }
                }

            }
        }

        // Si un objet a bien été trouvé, vérifier de quoi il s'agit.
        if (topSpriteRenderer != null)
        {
            // Si ce n'est pas un marqueur, retourner FAUX et change le state _objectInFront
            if (topSpriteRenderer.gameObject.layer != LayerMask.NameToLayer("Markers"))
            {
                _objectInFront = true;
                return false;
            }

            Vector2 markerWorldPos = topSpriteRenderer.gameObject.transform.position;
            Vector2 markerGridPos = GridCoords.FromWorldToGrid(markerWorldPos);
            _coordsText = markerGridPos.ToString();

            _currentTooltipPos = Camera.main.WorldToScreenPoint(markerWorldPos);
            _currentTooltipPos.x += 20f;
            _currentTooltipPos.y += 10f;

            return true;
        }
        // Sinon, retourner FAUX
        return false;
    }

    // Vérifier si un marker est sélectionné
    private bool MarkerIsSelected()
    {
        // Si aucun objet sélectionné
        if (_selectedObject == null)
        {
            return false;
        }
        // Sinon
        else
        {
            // Si c'est un marqueur
            if (_selectedObject.objectType == ObjectType.Marker)
            {
                // Assigner le texte
                _coordsText = GridCoords.FromWorldToGrid(_selectedObject.transform.position).ToString();

                // Assigner la position du tooltip
                _currentTooltipPos = Camera.main.WorldToScreenPoint(_selectedObject.transform.position);
                _currentTooltipPos.x += 20f;
                _currentTooltipPos.y += 10f;

                return true;
            }
            // Sinon faux
            return false;
        }
    }

    // Fonction qui garde une référence de l'objet sélectionné
    private void OnMovableObjectSelected(MovableObject obj)
    {
        _selectedObject = obj;
    }

    // Enlever la référence de l'objet sélectionné
    private void OnMovableObjectDeselected(MovableObject obj)
    {
        if (_selectedObject == obj)
            _selectedObject = null;
    }

    // Fonction qui update le tooltip et place sa position en fonction ce qui a été calculé.
    private void UpdateTooltip(Vector3 tooltipPos, string debugText)
    {
        // Si un objet se trouve devant, désactiver le texte
        if (_objectInFront)
        {
            if (_mouseCoordsText.enabled)
                _mouseCoordsText.enabled = false;

            return;
        }

        // Activer le texte si pas encore actif
        if (!_mouseCoordsText.enabled)
            _mouseCoordsText.enabled = true;

        _mouseCoordsText.transform.position = tooltipPos;
        _mouseCoordsText.text = debugText;
    }
}
