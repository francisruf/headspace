using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class DropZone : MonoBehaviour
{
    protected SpriteRenderer _containerSpriteRenderer;
    protected Collider2D _collider;

    [SerializeField] protected ObjectType[] acceptedObjects;
    public int ContainerSortingLayer { get { return _containerSpriteRenderer.sortingLayerID; } }
    public int HighestSortingOrder { get; private set; } = 0;

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider2D>();

        // Assigner le sprite renderer du parent, ou sinon celui de l'objet DropZone lui-même
        _containerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        if (_containerSpriteRenderer == null)
        {
            _containerSpriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public bool CheckIfAccepted(MovableObject obj)
    {
        for (int i = 0; i < acceptedObjects.Length; i++)
        {
            if (acceptedObjects[i] == obj.objectType)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 GetRandomPointInZone()
    {
        float randomX = Random.Range(_collider.bounds.min.x + 0.05f, _collider.bounds.max.x - 0.05f);
        float randomY = Random.Range(_collider.bounds.min.y + 0.05f, _collider.bounds.max.y - _collider.bounds.extents.y);

        return new Vector2(randomX, randomY);
    }

    public virtual void AddObjectToDropZone(MovableObject obj)
    {
        Bounds objBounds = obj.ColliderBounds;
        Vector2 centerOffset = objBounds.center - obj.transform.position;

        float minX = _collider.bounds.min.x + (objBounds.size.x / 2f) - centerOffset.x;
        float maxX = _collider.bounds.max.x - (objBounds.size.x / 2f) - centerOffset.x;
        float minY = _collider.bounds.min.y + (objBounds.size.y / 2f) - centerOffset.y;
        float maxY = _collider.bounds.max.y - (objBounds.size.y / 2f) - centerOffset.y;

        Vector2 newPos = obj.transform.position;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        obj.transform.position = newPos;
        obj.transform.parent = this.transform;
        HighestSortingOrder++;
        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(HighestSortingOrder);

        if (obj.Rigidbody != null)
        {
            obj.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public virtual void RemoveObjectFromDropZone(MovableObject obj)
    {
        if (obj.Rigidbody != null)
        {
            obj.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        obj.transform.parent = null;
        HighestSortingOrder--;
    }
}
