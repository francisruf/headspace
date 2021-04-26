using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolShadowRenderer : MonoBehaviour
{
    [Header("Shadow pixel offset")]
    public Vector2Int pixelOffset;

    private InteractableObject _interactableObject;
    private SpriteRenderer _spriteRenderer;
    private SpriteRenderer _shadowRenderer;

    private const float PIXEL_SIZE = 0.03125f;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _interactableObject = GetComponent<InteractableObject>();
    }

    private void Start()
    {
        GameObject shadow = new GameObject();
        shadow.gameObject.name = this.gameObject.name + "_shadow";
        shadow.transform.SetParent(this.transform);
        shadow.transform.localPosition = Vector2.zero;
        Vector2 position = shadow.transform.position;
        position.x += pixelOffset.x * PIXEL_SIZE;
        position.y -= pixelOffset.y * PIXEL_SIZE;
        shadow.transform.position = position;

        _shadowRenderer = shadow.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        _shadowRenderer.sprite = _spriteRenderer.sprite;
        Color shadowColor = Color.black;
        shadowColor.a = 0.2f;
        _shadowRenderer.color = shadowColor;
        _shadowRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;


        _shadowRenderer.sortingOrder = _spriteRenderer.sortingOrder - 1;
    }
}
