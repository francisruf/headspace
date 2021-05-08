using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyRenderer : MonoBehaviour
{
    public SpriteRenderer keyBaseRenderer;
    public SpriteRenderer buttonRenderer;
    public SpriteRenderer letterRenderer;
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    public void SetRenderingOrder(int order)
    {
        if (keyBaseRenderer != null)
            keyBaseRenderer.sortingOrder = order;
        if (buttonRenderer != null)
            buttonRenderer.sortingOrder = order + 1;
        if (letterRenderer != null)
            letterRenderer.sortingOrder = order + 2;
    }
}
