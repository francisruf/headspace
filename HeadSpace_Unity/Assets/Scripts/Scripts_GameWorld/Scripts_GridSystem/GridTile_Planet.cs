﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile_Planet : GridTile, PointOfInterest
{
    public static Action<GridTile_Planet> newPlanetTile;
    public static Action<GridTile_Planet> newPlanetFound;

    public PlanetInfo CurrentPlanetInfo { get; private set; }
    public string PlanetName;
    public PlanetSpriteMatch SpriteMatch { get; private set; }

    public GameObject planetRenderer;
    private SpriteRenderer _planetSpriteRenderer;
    private TextMeshProUGUI _planetNameText;
    private Animator _planetAnimator;
    private SpriteMask _planetMask;

    public int CurrentDistanceRating { get; set; }
    public bool PlanetFound { get; private set; }

    public List<Sprite> planetMaskSprites;
    private MapPointOfInterest _mapPointOfInterest;

    public int ContractHeat { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _planetSpriteRenderer = planetRenderer.GetComponent<SpriteRenderer>();
        _planetNameText = planetRenderer.GetComponentInChildren<TextMeshProUGUI>();
        _planetAnimator = planetRenderer.GetComponent<Animator>();
        _planetMask = planetRenderer.GetComponent<SpriteMask>();
        _mapPointOfInterest = GetComponentInChildren<MapPointOfInterest>();

        PlanetFound = false;
        _planetAnimator.SetBool("Visible", false);
        _planetNameText.enabled = false;
    }

    protected override void Start()
    {
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
    }

    public void AssignPlanetInfo(PlanetInfo info)
    {
        CurrentPlanetInfo = info;
        PlanetName = info.planetName;
        SpriteMatch = info.spriteMatch;

        if (_planetSpriteRenderer != null)
            _planetSpriteRenderer.sprite = SpriteMatch.mapSprite;
        if (_planetNameText != null)
        {
            _planetNameText.text = PlanetName;
            _planetNameText.ForceMeshUpdate();
        }
    }


    private void PlacePlanetRenderer()
    {
        float margin = 0.03125f;
        float planetRadius = _planetSpriteRenderer.sprite.bounds.extents.x;

        float minX = _spriteRenderer.bounds.min.x + planetRadius + margin;
        float maxX = _spriteRenderer.bounds.max.x - planetRadius - margin;
        float minY = _spriteRenderer.bounds.min.y + planetRadius + margin;
        float maxY = _spriteRenderer.bounds.max.y - planetRadius - margin;
        Vector2 randomPos = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        planetRenderer.transform.position = randomPos;

        SetStartingState(_mapPointOfInterest, true);
    }

    public void RevealPlanet(bool startOfGame)
    {
        if (PlanetFound)
            return;

        PlanetFound = true;

        if (newPlanetFound != null)
            newPlanetFound(this);

        if (!startOfGame)
        {
            StartCoroutine(RevealPlanetSprite());
        }
        else
        {
            SetStartingState(_mapPointOfInterest, false);
            _planetAnimator.SetBool("Visible", true);
            _planetNameText.enabled = true;
        }
    }

    private IEnumerator RevealPlanetSprite()
    {
        yield return _mapPointOfInterest.HideMapPointAnimation();

        int spriteCount = planetMaskSprites.Count;
        int count = 0;

        yield return new WaitForSeconds(0.1f);

        _planetSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        _planetAnimator.SetBool("Visible", true);

        while (count < spriteCount)
        {
            _planetMask.sprite = planetMaskSprites[count];
            count++;
            yield return new WaitForSeconds(1 / 60f);
        }
        _planetSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;

        if (TextHelper.instance != null)
            TextHelper.instance.TypeAnimation(_planetNameText);
    }

    public void RevealPoint(MapPointOfInterest point)
    {
        point.HideMapPoint();
    }

    public void SetStartingState(MapPointOfInterest point, bool isVisible)
    {
        _mapPointOfInterest.SetStartingState(isVisible, this);
    }

    public void AddContractHeat(int amount)
    {
        ContractHeat += amount;
    }
}
