using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_SpawningStaticAnomaly : GridTile
{
    private Animator _animator;
    public AnomalyPatch ParentPatch { get; set; }
    private bool _spawnBeforeStart;
    public SpriteRenderer baseTileRenderer;



    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();

        StartCoroutine(WaitRandom());

        if (!GameManager.GameStarted)
        {
            lifeTime = UnityEngine.Random.Range(0f, 0.6f);
            _spawnBeforeStart = true;
        }
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        base.InitializeTile(tileDimensions, gridMode);
        baseTileRenderer.size = tileDimensions;
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        base.InitializeTile(tileDimensions, gridMode, currentGridInfo);
        baseTileRenderer.size = tileDimensions;
    }

    private IEnumerator WaitRandom()
    {
        _animator.speed = 0f;
        float random = UnityEngine.Random.Range(0f, 1f);
        yield return new WaitForSeconds(random);
        _animator.speed = 1f;
    }

    protected override void LifeTimerOver()
    {
        base.LifeTimerOver();

        if (_spawnBeforeStart)
            GridManager.instance.ReplaceTile(this, 4);
    }

    protected override void AssignCheckeredSprite()
    {
        bool even = (tileX + tileY) % 2 == 0 ? true : false;
        if (even)
            baseTileRenderer.sprite = _gridInfo.evenSprite;
        else
            baseTileRenderer.sprite = _gridInfo.oddSprite;
    }
}
