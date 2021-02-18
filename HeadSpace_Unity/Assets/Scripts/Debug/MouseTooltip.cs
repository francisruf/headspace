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

    // Subscription à l'action newGameGrid
    private void OnEnable()
    {
        GridManager.newGameGrid += AssignGridInfo;
    }

    // Unsubscription
    private void OnDisable()
    {
        GridManager.newGameGrid -= AssignGridInfo;
    }

    private void Awake()
    {
        _mouseCoordsText = GetComponentInChildren<TextMeshProUGUI>();
        _mouseCoordsText.enabled = false;
    }

    // Assigne les valeurs de _currentGridInfo (appel à chaque nouvelle grille)
    private void AssignGridInfo(GridInfo info)
    {
        _currentGridInfo = info;
    }

    private void Update()
    {
        // Si gridInfo n'a pas été assigné encore, arrêter l'exécution
        if (_currentGridInfo == default)
            return;

        Vector3 mousePos = Vector3.zero;
        bool mouseOverGrid = MouseIsOverGrid(out mousePos);
        if (mouseOverGrid)
        {
            UpdateTooltip(mousePos);
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

        // Vérifier si la position se trouve dans les bounds de la grille.
        if (_currentGridInfo.gameGridWorldBounds.Contains(mousePosition))
        {
            return true;
        }
        return false;
    }

    private void UpdateTooltip(Vector3 mousePos)
    {
        // Activer le texte si pas encore actif
        if (!_mouseCoordsText.enabled)
            _mouseCoordsText.enabled = true;

        Vector3 tooltipPosition = Vector3.zero;
        tooltipPosition.x = Input.mousePosition.x;
        tooltipPosition.y = Input.mousePosition.y;

        _mouseCoordsText.transform.position = tooltipPosition;
        _mouseCoordsText.text = GridCoords.FromWorldToGrid(mousePos).ToString();
    }
}
