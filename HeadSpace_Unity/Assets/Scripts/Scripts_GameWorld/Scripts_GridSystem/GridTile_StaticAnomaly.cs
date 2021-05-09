using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_StaticAnomaly : GridTile
{
    public Action<GridTile_StaticAnomaly> anomalyTileDestroyed;
    public static Action firstAnomalySpawned;

    public static bool anomalySpawned;

    public SpriteRenderer idleTentaclesRenderer;
    public SpriteRenderer[] soloTentaclesRenderer = new SpriteRenderer[4];
    private Animator[] _soloTentaclesAnimators = new Animator[4];

    public SpriteRenderer corruptedTileRenderer;
    private Animator _corruptedTileAnimator;
    private TentacleAnimTrigger _animTrigger;

    [Header("Scissors settings")]
    public int totalHP;
    public int regenTickTime;
    public float regenStartCooldown;
    public int _currentHP;
    private IEnumerator _regenRoutine;
    private IEnumerator _displaceRoutine;
    public ParticleSystem psHit;
    public ParticleSystem psDead;
    private bool _destroyed;

    protected override void Awake()
    {
        base.Awake();
        _animTrigger = GetComponentInChildren<TentacleAnimTrigger>();
        _animTrigger.idleComplete += OnIdleComplete;
        _corruptedTileAnimator = corruptedTileRenderer.GetComponent<Animator>();
        _corruptedTileAnimator.SetFloat("Speed", 1.0f);
        for (int i = 0; i < 4; i++)
        {
            _soloTentaclesAnimators[i] = soloTentaclesRenderer[i].GetComponent<Animator>();
            _soloTentaclesAnimators[i].enabled = false;
            soloTentaclesRenderer[i].enabled = false;
        }

        _currentHP = totalHP;
    }

    //private void Update()
    //{
    //    if (!this.gameObject.activeSelf)
    //        return;

    //    if (Input.GetKeyDown(KeyCode.H))
    //        Hit();
    //}

    protected override void Start()
    {
        base.Start();
        if (!anomalySpawned && firstAnomalySpawned != null)
        {
            anomalySpawned = true;
            firstAnomalySpawned();
        }

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

    public void Hit(Vector2 hitPosition)
    {
        psHit.transform.position = hitPosition;
        psDead.transform.position = hitPosition;

        if (_regenRoutine != null)
        {
            StopCoroutine(_regenRoutine);
            _regenRoutine = null;
        }

        _regenRoutine = RegenHP();
        StartCoroutine(_regenRoutine);
        AddHP(-1);

        if (_currentHP > 0)
        {
            psHit.Play();
            foreach (var anim in _soloTentaclesAnimators)
            {
                anim.SetTrigger("Hit");
            }
            if (_displaceRoutine == null)
            {
                _displaceRoutine = Displace();
                StartCoroutine(_displaceRoutine);
            }
        }
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
        if (_destroyed)
            return;

        _destroyed = true;
        StartCoroutine(DestroyAnim());
    }

    private IEnumerator Displace()
    {
        Vector2[] startPos = new Vector2[4];

        for (int i = 0; i < 4; i++)
        {
            startPos[i] = soloTentaclesRenderer[i].transform.position;
            int randomMultiplier = UnityEngine.Random.Range(0, 10) > 4 ? -1 : 1;
            Vector2 newPos = startPos[i];
            newPos.x += randomMultiplier * 0.03125f;
            randomMultiplier = UnityEngine.Random.Range(0, 10) > 4 ? -1 : 1;
            newPos.y += randomMultiplier * 0.03125f;
            soloTentaclesRenderer[i].transform.position = newPos;
        }
        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < 4; i++)
        {
            soloTentaclesRenderer[i].transform.position = startPos[i];
        }
        _displaceRoutine = null;
    }

    private IEnumerator DestroyAnim()
    {
        psDead.Play();

        _corruptedTileAnimator.SetFloat("Speed", -2.0f);
        _corruptedTileAnimator.Play("Animaly_CorruptedTile_Spawn", 0, 0.99f);

        yield return new WaitForSeconds(1.1f);

        if (anomalyTileDestroyed != null)
            anomalyTileDestroyed(this);

        GridManager.instance.ReplaceTile(this, 0);
    }
}
