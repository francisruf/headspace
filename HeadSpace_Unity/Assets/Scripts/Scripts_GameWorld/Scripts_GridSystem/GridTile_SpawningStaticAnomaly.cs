using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile_SpawningStaticAnomaly : GridTile
{
    private Animator _animator;
    public AnomalyPatch ParentPatch { get; set; }
    private bool _spawnBeforeStart;

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
}
