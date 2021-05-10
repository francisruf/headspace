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
    [Header("Color settings")]
    public Color letterColor;
    public Color numberColor;

    [Header("Display settings")]
    public bool topXCoordinates;
    public bool bottomXCoordinates;
    public bool leftYCoordinates;
    public bool rightYCoordinates;

    [Header("Misc")]
    public GameObject coordinatesTextPrefab;
    public float coordinatesOffsetFromMap;

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer crossRenderer;
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
        Vector2 margins = new Vector2(0.34375f, 0.34375f);

        _currentGridInfo = gridInfo;
        //_spriteRenderer.size = (Vector2)_currentGridInfo.gameGridWorldBounds.size + (margins * 2f);
        Vector2 size = new Vector2();
        size.x = _currentGridInfo.tileWidth * _currentGridInfo.gameGridSize.x;
        size.y = _currentGridInfo.tileWidth * _currentGridInfo.gameGridSize.y;
        _spriteRenderer.size = size + (margins * 2f);
        Vector2 pos = Vector2.zero;
        pos.x += 0.03125f;
        pos.y -= 0.03125f;
        transform.position = pos;

        Vector2 crossStartPos = _currentGridInfo.gameGridWorldBounds.min;
        crossStartPos.y = _currentGridInfo.gameGridWorldBounds.max.y;
        crossStartPos.x -= 0.03125f;
        crossStartPos.y += 0.03125f;
        crossRenderer.transform.position = crossStartPos;
        crossRenderer.size = (Vector2)_currentGridInfo.gameGridWorldBounds.size + new Vector2(0.59375f, 0.59375f);

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
                spawnPos.y -= 0.03125f;
                TextMeshProUGUI txt = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                txt.text = (i + 1).ToString();
                txt.transform.position = spawnPos;
                txt.color = numberColor;
                _allCoordinateTexts.Add(txt);
            }

            if (topXCoordinates)
            {
                TextMeshProUGUI txt2 = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                spawnPos.y = _currentGridInfo.gameGridWorldBounds.max.y + coordinatesOffsetFromMap;
                txt2.text = (i + 1).ToString();
                txt2.transform.position = spawnPos;
                txt2.color = numberColor;
                _allCoordinateTexts.Add(txt2);
            }
        }

        // Spawn Y
        for (int i = 0; i < _currentGridInfo.gameGridSize.y; i++)
        {
            Vector2 spawnPos = new Vector2();
            spawnPos.x = _currentGridInfo.gameGridWorldBounds.min.x - coordinatesOffsetFromMap;
            spawnPos.y = _currentGridInfo.gameGridWorldBounds.max.y;
            spawnPos.y -= (tileSize / 2f);
            spawnPos.y -= (tileSize * i);

            if (leftYCoordinates)
            {
                TextMeshProUGUI txt = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                txt.text = GridCoords.GetTileLetter(i);
                txt.transform.position = spawnPos;
                txt.color = letterColor;
                _allCoordinateTexts.Add(txt);
            }

            if (rightYCoordinates)
            {
                

                TextMeshProUGUI txt2 = Instantiate(coordinatesTextPrefab, _coordinatesCanvas.transform).GetComponent<TextMeshProUGUI>();
                spawnPos.x = _currentGridInfo.gameGridWorldBounds.max.x + coordinatesOffsetFromMap;
                spawnPos.x += 0.0625f;
                txt2.text = GridCoords.GetTileLetter(i);
                txt2.transform.position = spawnPos;
                txt2.color = letterColor;
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
