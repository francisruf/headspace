using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DropZone : MonoBehaviour
{
    private SpriteRenderer _containerSpriteRenderer;
    private Collider2D _collider;

    [SerializeField] private ObjectType[] acceptedObjects;
    public int ContainerSortingLayer { get { return _containerSpriteRenderer.sortingLayerID; } }
    public int HighestSortingOrder { get; private set; } = 0;

    private void Awake()
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

    public void AddObjectToDropZone(MovableObject obj)
    {
        Bounds objBounds = obj.ColliderBounds;
        float minX = _collider.bounds.min.x + (objBounds.size.x / 2f);
        float maxX = _collider.bounds.max.x - (objBounds.size.x / 2f);
        float minY = _collider.bounds.min.y + (objBounds.size.y / 2f);
        float maxY = _collider.bounds.max.y - (objBounds.size.y / 2f);

        Vector2 newPos = obj.transform.position;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        obj.transform.position = newPos;
        obj.transform.parent = this.transform;
        HighestSortingOrder++;
        obj.SetSortingLayer(ContainerSortingLayer);
        obj.SetOrderInLayer(HighestSortingOrder);
    }

    public void RemoveObjectFromDropZone(MovableObject obj)
    {
        obj.transform.parent = null;
        HighestSortingOrder--;
    }
}
