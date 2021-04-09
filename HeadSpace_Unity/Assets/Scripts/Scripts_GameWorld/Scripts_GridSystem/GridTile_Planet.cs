using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile_Planet : GridTile
{
    public static Action<GridTile_Planet> newPlanetTile;

    public PlanetInfo CurrentPlanetInfo { get; private set; }
    public string PlanetName { get; private set; }
    public Sprite PlanetSprite { get; private set; }

    public GameObject planetRenderer;
    private SpriteRenderer _planetSpriteRenderer;
    private TextMeshProUGUI _planetNameText;

    protected override void Awake()
    {
        base.Awake();
        _planetSpriteRenderer = planetRenderer.GetComponent<SpriteRenderer>();
        _planetNameText = planetRenderer.GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        base.InitializeTile(tileDimensions, gridMode);
        PlacePlanetRenderer();

        if (newPlanetTile != null)
            newPlanetTile(this);
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        base.InitializeTile(tileDimensions, gridMode, currentGridInfo);

        if (newPlanetTile != null)
            newPlanetTile(this);
    }

    public void AssignPlanetInfo(PlanetInfo info)
    {
        CurrentPlanetInfo = info;
        PlanetName = info.planetName;
        PlanetSprite = info.planetSprite;

        if (_planetSpriteRenderer != null)
            _planetSpriteRenderer.sprite = PlanetSprite;
        if (_planetNameText != null)
            _planetNameText.text = PlanetName;
    }

    private void PlacePlanetRenderer()
    {
        float margin = 0.0625f;
        float planetRadius = _planetSpriteRenderer.sprite.bounds.extents.x;

        float minX = _spriteRenderer.bounds.min.x + planetRadius + margin;
        float maxX = _spriteRenderer.bounds.max.x - planetRadius - margin;
        float minY = _spriteRenderer.bounds.min.y + planetRadius + margin;
        float maxY = _spriteRenderer.bounds.max.y - planetRadius - margin;
        Vector2 randomPos = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        planetRenderer.transform.position = randomPos;
    }
}
