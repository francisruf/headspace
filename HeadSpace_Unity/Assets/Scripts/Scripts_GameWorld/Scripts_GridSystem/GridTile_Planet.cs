using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GridTile_Planet : GridTile
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

    public int DistanceRating { get; private set; }
    public bool PlanetFound { get; private set; }

    public List<Sprite> planetMaskSprites;

    protected override void Awake()
    {
        base.Awake();
        _planetSpriteRenderer = planetRenderer.GetComponent<SpriteRenderer>();
        _planetNameText = planetRenderer.GetComponentInChildren<TextMeshProUGUI>();
        _planetAnimator = planetRenderer.GetComponent<Animator>();
        _planetMask = planetRenderer.GetComponent<SpriteMask>();

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

        CalculateDistance();
    }

    private void CalculateDistance()
    {
        GridTile_DeployPoint point = DeployManager.instance.CurrentDeployTile;
        List<PathNode> pathToDeploy = PathFinder.instance.FindPath(tileX, tileY, point.tileX, point.tileY);

        if (pathToDeploy != null)
            DistanceRating = pathToDeploy[pathToDeploy.Count-1].fCost;
        else
            DistanceRating = 1000;
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

    public void RevealPlanet()
    {
        if (PlanetFound)
            return;

        PlanetFound = true;

        if (newPlanetFound != null)
            newPlanetFound(this);

        StartCoroutine(RevealPlanetSprite());
    }

    private IEnumerator RevealPlanetSprite()
    {
        int spriteCount = planetMaskSprites.Count;
        int count = 0;

        _planetSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        _planetAnimator.SetBool("Visible", true);

        while (count < spriteCount)
        {
            _planetMask.sprite = planetMaskSprites[count];
            count++;
            yield return new WaitForEndOfFrame();
        }
        _planetSpriteRenderer.maskInteraction = SpriteMaskInteraction.None;

        if (TextHelper.instance != null)
            TextHelper.instance.TypeAnimation(_planetNameText);
    }
}
