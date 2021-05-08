using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class DropZone : MonoBehaviour
{
    public int minSortingOrder;
    public string targetSortingLayer;

    protected SpriteRenderer _containerSpriteRenderer;
    protected Collider2D _collider;
    protected Collider2D _activeCollider;
    public Bounds ColliderBounds { get { return _collider.bounds; } }

    [SerializeField] protected ObjectType[] acceptedObjects;
    public int ContainerSortingLayer { get { return _containerSpriteRenderer.sortingLayerID; } }
    public int HighestSortingOrder { get; protected set; } = 0;

    protected int _objectCount;

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _activeCollider = _collider;
        // Assigner le sprite renderer du parent, ou sinon celui de l'objet DropZone lui-même
        _containerSpriteRenderer = GetComponentInParent<SpriteRenderer>();
        if (_containerSpriteRenderer == null)
        {
            _containerSpriteRenderer = GetComponent<SpriteRenderer>();
        }
        HighestSortingOrder = minSortingOrder;
    }

    public virtual bool CheckIfAccepted(MovableObject obj)
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
        float randomX = Random.Range(_activeCollider.bounds.min.x + 0.05f, _activeCollider.bounds.max.x - 0.05f);
        float randomY = Random.Range(_activeCollider.bounds.min.y + 0.05f, _activeCollider.bounds.max.y - _activeCollider.bounds.extents.y);

        return new Vector2(randomX, randomY);
    }

    public virtual void AddObjectToDropZone(MovableObject obj)
    {
        Bounds objBounds = obj.ColliderBounds;
        Vector2 centerOffset = objBounds.center - obj.transform.position;

        float minX = _activeCollider.bounds.min.x + (objBounds.size.x / 2f) - centerOffset.x;
        float maxX = _activeCollider.bounds.max.x - (objBounds.size.x / 2f) - centerOffset.x;
        float minY = _activeCollider.bounds.min.y + (objBounds.size.y / 2f) - centerOffset.y;
        float maxY = _activeCollider.bounds.max.y - (objBounds.size.y / 2f) - centerOffset.y;

        Vector2 newPos = obj.transform.position;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        obj.transform.position = newPos;
        obj.transform.parent = this.transform;

        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(HighestSortingOrder);
        HighestSortingOrder = obj.GetHighestOrder() + 1;

        //if (obj.Rigidbody != null)
        //{
        //    obj.Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        //}
        _objectCount++;
    }

    public virtual void RemoveObjectFromDropZone(MovableObject obj)
    {
        //if (obj.Rigidbody != null)
        //{
        //    obj.Rigidbody.bodyType = RigidbodyType2D.Kinematic;
        //}

        obj.transform.parent = null;
        _objectCount--;
    }
}
