using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_StaticAnomaly : GridTile
{
    public Action<GridTile_StaticAnomaly> anomalyTileDestroyed;

    public SpriteRenderer idleTentaclesRenderer;
    public SpriteRenderer[] soloTentaclesRenderer = new SpriteRenderer[4];
    private Animator[] _soloTentaclesAnimators = new Animator[4];

    public SpriteRenderer corruptedTileRenderer;
    private TentacleAnimTrigger _animTrigger;

    [Header("Scissors settings")]
    public int totalHP;
    public int regenTickTime;
    public float regenStartCooldown;
    public int _currentHP;
    private IEnumerator _regenRoutine;

    protected override void Awake()
    {
        base.Awake();
        _animTrigger = GetComponentInChildren<TentacleAnimTrigger>();
        _animTrigger.idleComplete += OnIdleComplete;

        for (int i = 0; i < 4; i++)
        {
            _soloTentaclesAnimators[i] = soloTentaclesRenderer[i].GetComponent<Animator>();
            _soloTentaclesAnimators[i].enabled = false;
            soloTentaclesRenderer[i].enabled = false;
        }

        _currentHP = totalHP;
    }

    private void Update()
    {
        if (!this.gameObject.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.H))
            Hit();
    }

    private void OnDisable()
    {
        _animTrigger.idleComplete -= OnIdleComplete;
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode)
    {
        base.InitializeTile(tileDimensions, gridMode);
        corruptedTileRenderer.size = tileDimensions;
        SetTentaclePosition();
    }

    public override void InitializeTile(Vector2 tileDimensions, GridMode gridMode, GridInfo currentGridInfo)
    {
        base.InitializeTile(tileDimensions, gridMode, currentGridInfo);
        corruptedTileRenderer.size = tileDimensions;
        SetTentaclePosition();
    }

    private void SetTentaclePosition()
    {
        Vector2 pos = _spriteRenderer.bounds.min;
        pos.x = _spriteRenderer.bounds.max.x;
        //Debug.DrawLine(_spriteRenderer.bounds.min, _spriteRenderer.bounds.min + Vector3.up * 0.5f, Color.yellow);
        idleTentaclesRenderer.transform.position = pos;
        foreach (var renderer in soloTentaclesRenderer)
        {
            renderer.transform.position = pos;
        }
    }

    private void OnIdleComplete()
    {
        for (int i = 0; i < 4; i++)
        {
            idleTentaclesRenderer.enabled = false;
            soloTentaclesRenderer[i].enabled = true;
            _soloTentaclesAnimators[i].enabled = true;
        }
    }

    private void Hit()
    {
        if (_regenRoutine != null)
        {
            StopCoroutine(_regenRoutine);
            _regenRoutine = null;
        }

        _regenRoutine = RegenHP();
        StartCoroutine(_regenRoutine);

        AddHP(-1);
    }

    private IEnumerator RegenHP()
    {
        yield return new WaitForSeconds(regenStartCooldown);

        while (_currentHP < totalHP)
        {
            AddHP(1);
            yield return new WaitForSeconds(regenTickTime);
        }
        _regenRoutine = null;
    }

    private void AddHP(int amount)
    {
        _currentHP = Mathf.Clamp(_currentHP + amount, 0, totalHP);

        int rendererAmount = Mathf.CeilToInt(_currentHP / 5f);
        for (int i = 0; i < 4; i++)
        {
            soloTentaclesRenderer[i].enabled = i <= rendererAmount - 1;
        }

        if (_currentHP == totalHP)
        {
            if (_regenRoutine != null)
            {
                StopCoroutine(_regenRoutine);
                _regenRoutine = null;
            }
        }
        else if (_currentHP == 0)
        {
            DestroyTile();
        }
    }

    private void DestroyTile()
    {
        if (anomalyTileDestroyed != null)
            anomalyTileDestroyed(this);

        GridManager.instance.ReplaceTile(this, 0);
    }
}
