using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPointOfInterest : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private SpriteMask _spriteMask;
    public List<Sprite> maskSprites;

    private bool _isVisible;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteMask = GetComponent<SpriteMask>();

        _isVisible = true;
    }

    public void SetStartingState(bool isVisible, GridTile targetTile)
    {
        _isVisible = isVisible;
        _spriteRenderer.enabled = _isVisible;

        float offset = 1 / 32f;
        float minX = targetTile.TileBounds.min.x + _spriteRenderer.bounds.extents.x;
        float maxX = targetTile.TileBounds.max.x - _spriteRenderer.bounds.extents.x;
        float minY = targetTile.TileBounds.min.y + _spriteRenderer.bounds.extents.y + offset;
        float maxY = targetTile.TileBounds.max.y - _spriteRenderer.bounds.extents.y - offset;

        transform.position = new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
    }

    public void HideMapPoint()
    {
        if (!_isVisible)
            return;

        StartCoroutine(HideMapPointAnimation());
    }

    public IEnumerator HideMapPointAnimation()
    {
        _isVisible = false;

        int spriteCount = maskSprites.Count;
        int count = 0;

        _spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        //_animator.SetBool("Visible", true);

        while (count < spriteCount)
        {
            _spriteMask.sprite = maskSprites[count];
            count++;
            yield return new WaitForSeconds(1/45f);
        }
        _spriteRenderer.enabled = false;
        _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        _spriteMask.enabled = false;
    }
}
