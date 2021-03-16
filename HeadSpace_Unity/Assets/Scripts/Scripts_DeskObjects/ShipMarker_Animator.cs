using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipMarker_Animator : MonoBehaviour
{
    private Animator _animator;
    private ShipMarker _shipMarker;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _shipMarker = GetComponentInChildren<ShipMarker>();
    }

    public void TriggerDestroy()
    {
        _animator.SetTrigger("Destroy");
    }

    public void DestroyAnimationComplete()
    {
        //_shipMarker.OnDestroyAnimationComplete();
    }
}
