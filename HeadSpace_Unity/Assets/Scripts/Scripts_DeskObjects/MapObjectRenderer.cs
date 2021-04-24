using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectRenderer : MonoBehaviour
{
    private float _sortingOrderBase = 5000f;

    private bool _runOnlyOnce = true;
    private bool _executed = false;
    public Renderer targetRenderer;

    private void LateUpdate()
    {
        if (_executed)
            return;

        targetRenderer.sortingOrder = (int)(_sortingOrderBase - (transform.position.y * 32));
        if (_runOnlyOnce)
            _executed = true;
    }
}
