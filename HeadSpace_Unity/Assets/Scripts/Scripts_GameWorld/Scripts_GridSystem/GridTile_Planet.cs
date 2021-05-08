using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    private Image _planetLabel;
    private Canvas _planetCanvas;

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
        _planetLabel = planetRenderer.GetComponentInChildren<Image>();
        _planetMask = planetRenderer.GetComponent<SpriteMask>();
        _planetCanvas = planetRenderer.GetComponent<Canvas>();
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

            StartCoroutine(AssignPlanetLabel());
        }
    }

    private IEnumerator AssignPlanetLabel()
    {
        yield return new WaitForEndOfFrame();

        RectTransform planetRect = _planetLabel.GetComponent<RectTransform>();
        Vector2 newSize = planetRect.sizeDelta;
        newSize.x = _planetNameText.textBounds.size.x + 0.06f;
        planetRect.sizeDelta = newSize;

        Vector3[] corners = new Vector3[4];
        planetRect.GetWorldCorners(corners);

        if (corners[0].x < GridCoords.CurrentGridInfo.gameGridWorldBounds.min.x)
        {
            float offset = GridCoords.CurrentGridInfo.gameGridWorldBounds.min.x - corners[0].x + 0.0625f;
            Vector2 newRectPos = planetRect.transform.position;
            Vector2 newTextPos = _planetNameText.transform.position;
            newRectPos.x += offset;
            newTextPos.x += offset;
            planetRect.transform.position = newRectPos;
            _planetNameText.transform.position = newTextPos;
        }

        else if (corners[3].x > GridCoords.CurrentGridInfo.gameGridWorldBounds.max.x)
        {
            float offset = GridCoords.CurrentGridInfo.gameGridWorldBounds.max.x - corners[3].x - 0.03125f;
            Vector2 newRectPos = planetRect.transform.position;
            Vector2 newTextPos = _planetNameText.transform.position;
            newRectPos.x += offset;
            newTextPos.x += offset;
            planetRect.transform.position = newRectPos;
            _planetNameText.transform.position = newTextPos;
        }
    }

    private void PlacePlanetRenderer()
    {
        float margin = 0.03125f;
        float planetRadius = _planetSpriteRenderer.sprite.bounds.extents.x;

        float minX = _spriteRenderer.bounds.min.x + planetRadius + margin;
        float maxX = _spriteRenderer.bounds.max.x - planetRadius - margin;
        float posY = _spriteRenderer.bounds.center.y + 0.09375f;

        Vector2 planetPos = new Vector2(UnityEngine.Random.Range(minX, maxX), posY);
        planetRenderer.transform.position = planetPos;
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
