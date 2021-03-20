/* Classe qui sert principalement à resize la worldMap (la texture d'arrière-plan) en fonction de la grille 
 * qui est générée.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class DynamicWorldMap : MonoBehaviour
{
    public bool topXCoordinates;
    public bool bottomXCoordinates;
    public bool leftYCoordinates;
    public bool rightYCoordinates;

    public GameObject coordinatesTextPrefab;
    public float coordinatesOffsetFromMap;

    private SpriteRenderer _spriteRenderer;
    private GridInfo _currentGridInfo;
    private Canvas _coordinatesCanvas;

    private List<TextMeshProUGUI> _allCoordinateTexts = new List<TextMeshProUGUI>();

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _coordinatesCanvas = GetComponentInChildren<Canvas>();
    }

    private void OnEnable()
    {
        GridManager.newGameGrid += InitializeWorldMap;
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
    }

    private void OnDisable()
    {
        GridManager.newGameGrid -= InitializeWorldMap;
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
    }

    private void InitializeWorldMap(GridInfo gridInfo)
    {
        _currentGridInfo = gridInfo;
        _spriteRenderer.size = _currentGridInfo.gameGridWorldBounds.size;

        AssignCoordinatesText();
    }

    private void AssignCoordinatesText()
    {
        float tileSize = _currentGridInfo.gameGridWorldBounds.size.x / _currentGridInfo.gameGridSize.x;

        // Spawn X
        for (int i = 0; i < _currentGridInfo.gameGridSize.x; i++)
        {
            Vector2 spawnPos = new Vector2();
            spawnPos.y = _currentGridInfo.gameGridWorldBounds.min.y - coordinatesOffsetFromMap;
            spawnPos.x = _currentGridInfo.gameGridWorldBounds.min.x;
            spawnPos.x += (tileSize / 2f);
            spawnPos.x += (tileSize * i);

            if (bottomXCoordinates)
            {
                TextMeshProUGUI txt = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                txt.text = i.ToString();
                txt.transform.position = spawnPos;
                _allCoordinateTexts.Add(txt);
            }

            if (topXCoordinates)
            {
                TextMeshProUGUI txt2 = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                spawnPos.y = _currentGridInfo.gameGridWorldBounds.max.y + coordinatesOffsetFromMap;
                txt2.text = i.ToString();
                txt2.transform.position = spawnPos;
                _allCoordinateTexts.Add(txt2);
            }
        }

        // Spawn Y
        for (int i = 0; i < _currentGridInfo.gameGridSize.y; i++)
        {
            Vector2 spawnPos = new Vector2();
            spawnPos.x = _currentGridInfo.gameGridWorldBounds.min.x - coordinatesOffsetFromMap;
            spawnPos.y = _currentGridInfo.gameGridWorldBounds.min.y;
            spawnPos.y += (tileSize / 2f);
            spawnPos.y += (tileSize * i);

            if (leftYCoordinates)
            {
                TextMeshProUGUI txt = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                txt.text = i.ToString();
                txt.transform.position = spawnPos;
                _allCoordinateTexts.Add(txt);
            }

            if (rightYCoordinates)
            {
                TextMeshProUGUI txt2 = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                spawnPos.x = _currentGridInfo.gameGridWorldBounds.max.x + coordinatesOffsetFromMap;
                txt2.text = i.ToString();
                txt2.transform.position = spawnPos;

                _allCoordinateTexts.Add(txt2);
            }
        }
    }

    private void OnGridDataDestroyed()
    {
        foreach (var txt in _allCoordinateTexts)
        {
            Destroy(txt.gameObject);
        }
        _allCoordinateTexts.Clear();
    }
}
