using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractBoardSlot : MonoBehaviour
{
    protected Animator _animator;
    protected Collider2D _collider;
    protected SpriteRenderer _spriteRenderer;
    public DropZone_ContractSlot[] contractDropZones;

    protected virtual void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        int count = contractDropZones.Length;
        for (int i = 0; i < count; i++)
        {
            contractDropZones[i].NeighbourZones = contractDropZones;
            contractDropZones[i].ZoneIndex = i;
            contractDropZones[i].NeighbourCount = count;
        }
    }

    public void DisableSlots()
    {
        _collider.enabled = false;
        _spriteRenderer.enabled = false;
    }

    public void AssignState(bool isActive)
    {
        _animator.SetBool("IsActive", isActive);
    }
}
