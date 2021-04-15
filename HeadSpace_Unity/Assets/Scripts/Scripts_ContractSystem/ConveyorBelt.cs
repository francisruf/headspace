using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    public Bounds Bounds { get { return _spriteRenderer.bounds; } }
    public Transform contractsStartPos;
    public Transform contractsEndPos;

    private List<MovableContract> _contractsOnBelt = new List<MovableContract>();
    private int _contractsOnBeltCount;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        MovableContract.contractOnBelt += OnNewContractOnBelt;
        MovableContract.contractExitBelt += OnContractRemovedFromBelt;
    }

    private void OnDisable()
    {
        MovableContract.contractOnBelt -= OnNewContractOnBelt;
        MovableContract.contractExitBelt -= OnContractRemovedFromBelt;
    }

    private void OnNewContractOnBelt(MovableContract contract)
    {
        _contractsOnBelt.Add(contract);
        _contractsOnBeltCount++;

        AssignState();
    }

    private void OnContractRemovedFromBelt(MovableContract contract)
    {
        _contractsOnBelt.Remove(contract);
        _contractsOnBeltCount--;

        AssignState();
    }

    private void AssignState()
    {
        if (_contractsOnBeltCount > 0)
            _animator.SetBool("IsActive", true);
        else
            _animator.SetBool("IsActive", false);
    }
}
