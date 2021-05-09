using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shredder : StaticTool
{
    private ShredderSlot child = null;
    private Vector2 _startPos;

    [Header("Noise settings")]
    public float heightScale = 0.03125f;
    public float xScale = 1.0f;

    [Header("Cover sprites")]
    public Sprite closedSprite;
    public Sprite openSprite;
    public SpriteRenderer coverRenderer;

    private void OnEnable()
    {
        TutorialController.shredderEnableRequest += OnShredderEnableRequest;
    }

    private void OnDisable()
    {
        TutorialController.shredderEnableRequest -= OnShredderEnableRequest;
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        child = transform.GetComponentInChildren<ShredderSlot>();
        _startPos = transform.position;

        if (startEnabled)
            coverRenderer.sprite = openSprite;
        else
            coverRenderer.sprite = closedSprite;
    }

    protected override void Start()
    {
        base.Start();
        child.canShred = startEnabled;
        child.UpdateLightState();
    }

    private void Update()
    {
        if (child.shredding)
        {
            float height = heightScale * Mathf.PerlinNoise(Time.time * xScale, 0.0f);
            Vector3 pos = transform.position;
            pos.y = _startPos.y + height;
            transform.position = pos;
        }
        else
        {
            transform.position = _startPos;
        }

    }

    private void OnShredderEnableRequest()
    {
        ToggleInteractions(true);
        child.TriggerLightsEnable();
        coverRenderer.sprite = openSprite;
    }

    public override void ToggleInteractions(bool toggleON)
    {
        _interactionsEnabled = toggleON;
        child.canShred = true;
        child.UpdateLightState();
    }

    public override void DisableObject()
    {
        base.DisableObject();
        Debug.Log("AS:DLK");
    }
}
