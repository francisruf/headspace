using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectExclusionZone : MonoBehaviour
{
    private Collider2D _collider;
    public ClampDirection clampDirection;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    public void ClampObject(MovableObject obj)
    {
        switch (clampDirection)
        {
            case ClampDirection.Up:
                if (obj.ColliderBounds.center.y < _collider.bounds.max.y)
                {
                    Vector2 newPos = obj.transform.position;
                    newPos.y = _collider.bounds.max.y;
                    newPos.y -= (obj.ColliderBounds.center.y - obj.transform.position.y);
                    obj.transform.position = newPos;
                }
                break;
            case ClampDirection.Down:
                if (obj.ColliderBounds.center.y > _collider.bounds.min.y)
                {
                    Vector2 newPos = obj.transform.position;
                    newPos.y = _collider.bounds.min.y;
                    newPos.y -= (obj.ColliderBounds.center.y - obj.transform.position.y);
                    obj.transform.position = newPos;
                }
                break;
            case ClampDirection.Left:
                if (obj.ColliderBounds.center.x > _collider.bounds.min.x)
                {
                    Vector2 newPos = obj.transform.position;
                    newPos.x = _collider.bounds.min.x;
                    newPos.x -= (obj.ColliderBounds.center.x - obj.transform.position.x);
                    obj.transform.position = newPos;
                }
                break;
            case ClampDirection.Right:
                if (obj.ColliderBounds.center.x < _collider.bounds.max.x)
                {
                    Vector2 newPos = obj.transform.position;
                    newPos.x = _collider.bounds.max.x;
                    newPos.x -= (obj.ColliderBounds.center.x - obj.transform.position.x);
                    obj.transform.position = newPos;
                }
                break;
            default:
                break;
        }
    }
}

public enum ClampDirection
{
    Up,
    Down,
    Left,
    Right
}
