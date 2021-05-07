// TODO : Function that verifies perdiodically if object is in allowed position after screen resize
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovableObject : InteractableObject
{
    public static Action<MovableObject> movableObjectSelected;
    public static Action<MovableObject> movableObjectDeselected;
    public static Action<MovableObject> movableObjectMoving;
    public static Action<MovableObject, ObjectSpawnZone> placeObjectRequest;

    protected SpriteRenderer _shadowRenderer;
    protected Rigidbody2D _rigidBody;
    public Rigidbody2D Rigidbody { get { return _rigidBody; } }

    public ObjectType objectType;

    // DropZone (container) dans lequel l'objet se trouve
    protected DropZone _currentDropZone;
    public LayerMask dropZoneLayerMask;

    // Position de la souris lorsque l'objet est cliqué
    protected Vector2 _mouseOffset;

    // Valeurs minimales et maximales pour le déplacement de l'objet
    private float _clampXmin;
    private float _clampXmax;
    private float _clampYmin;
    private float _clampYmax;

    private IEnumerator _movementRoutine;

    protected override void Awake()
    {
        base.Awake();
        _rigidBody = GetComponentInChildren<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();

        
        GameObject shadow = new GameObject();
        shadow.gameObject.name = this.gameObject.name + "_shadow";
        shadow.transform.SetParent(this.transform);
        shadow.transform.localPosition = Vector2.zero;
        Vector2 position = shadow.transform.position;
        position.x += ((1 / 32f) * 1);
        position.y -= ((1 / 32f) * 3);
        shadow.transform.position = position;

        _shadowRenderer = shadow.AddComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        _shadowRenderer.sprite = _spriteRenderer.sprite;
        Color shadowColor = Color.black;
        shadowColor.a = 0.2f;
        _shadowRenderer.color = shadowColor;
        _shadowRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
        _shadowRenderer.sortingOrder = GetOrderInLayer();
        _shadowRenderer.enabled = false;
    }

    protected virtual void Update()
    {
        if (_isSelected)
        {
            // Tant que l'objet est sélectionné et que le bouton de souris est enfoncé
            if (Input.GetMouseButton(0))
            {
                MoveObject();
            }
            // Sinon, déselectionner
            else
            {
                Deselect();
            }
        }
    }

    // Fonction appelée par le ObjectsManager
    public override void Select(bool fireEvent = true)
    {
        if (_currentDropZone != null)
        {
            RemoveFromDropZone();
            SetSortingLayer(SortingLayer.NameToID("SelectedObject"));
            _isSelected = true;
        }
        else
        {
            base.Select(fireEvent);
        }

        _mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateMinMaxPosition();

        _movementRoutine = CheckMovement();
        StartCoroutine(_movementRoutine);

        if (_shadowRenderer != null)
            _shadowRenderer.enabled = true;

        if (fireEvent)
            if (movableObjectSelected != null)
                movableObjectSelected(this);
    }

    // Fonction qui force l'objet à être déposé
    public override void Deselect(bool fireEvent = true)
    {
        if (CheckForDropZone(out _currentDropZone))
        {
            _isSelected = false;
            AssignToDropZone();

            if (interactableDeselected != null)
                interactableDeselected(this);
        }
        else
        {
            base.Deselect(fireEvent);
        }

        if (_movementRoutine != null)
        {
            StopCoroutine(_movementRoutine);
            _movementRoutine = null;
        }

        if (_shadowRenderer != null)
            _shadowRenderer.enabled = false;

        if (fireEvent)
            if (movableObjectDeselected != null)
                movableObjectDeselected(this);
    }

    // Fonction qui désactive l'objet (fonctionnalité complète dans classe de base - InteractableObject)
    public override void DisableObject()
    {
        base.DisableObject();

        if (_isSelected)
        {
            Deselect();
        }
    }

    protected virtual bool CheckForDropZone(out DropZone dropZone)
    {
        dropZone = null;
        bool found = false;

        List<Collider2D> allOverlappedColliders = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(dropZoneLayerMask);

        int colliderCount = _collider.OverlapCollider(filter, allOverlappedColliders);

        for (int i = 0; i < colliderCount; i++)
        {
            DropZone candidate = allOverlappedColliders[i].GetComponent<DropZone>();
            
            if (candidate != null)
            {
                if (candidate.CheckIfAccepted(this))
                {
                    dropZone = candidate;
                    found = true;
                    break;
                }
            }
        }
        return found;
    }

    public void ForceDropZone(DropZone zone)
    {
        _currentDropZone = zone;
    }

    protected virtual void AssignToDropZone()
    {
        _currentDropZone.AddObjectToDropZone(this);
    }

    protected virtual void RemoveFromDropZone()
    {
        _currentDropZone.RemoveObjectFromDropZone(this);
        _currentDropZone = null;
    }

    // Fonction principale qui bouge l'objet (appelée dans Update à chaque frame)
    private void MoveObject()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 newPos = mouseWorldPos + _mouseOffset;
        this.transform.position = ClampPosition(newPos);
    }

    // Fonction qui utilise les valeurs min/max pour barrer la position de l'objet à l'intérieur de l'écran
    private Vector2 ClampPosition(Vector2 unclampedPos)
    {
        Vector2 clampedPos = unclampedPos;
        clampedPos.x = Mathf.Clamp(clampedPos.x, _clampXmin, _clampXmax);
        clampedPos.y = Mathf.Clamp(clampedPos.y, _clampYmin, _clampYmax);

        return clampedPos;
    }

    // Fonction qui calcule les valeurs de dépassement maximal en x,y d'un objet, en fonction de : 
    // -- La taille de l'écran (fonctionne avec tous les formats)
    // -- Les dimensions de l'objets, calculées à partir des BOUNDS de son collider
    // La fonction assigne ces valeurs lorsque l'objet devient actif.
    private void CalculateMinMaxPosition()
    {
        float vertExtent = Camera.main.orthographicSize;
        float horExtent = vertExtent * Screen.width / Screen.height;

        Bounds screenBounds = new Bounds(Camera.main.transform.position, new Vector3((horExtent * 2f) - 1f, (vertExtent * 2f) - 1f, 0f));
        Bounds objectBounds = _collider.bounds;


        // SI la largeur de l'objet est plus petite que 0.5f, le depassement maximal X de l'objet est de la moitié de sa largeur
        // (transform.position donne un point au CENTRE de l'objet)
        if (objectBounds.size.x < 0.5f)
        {
            _clampXmin = screenBounds.min.x;
            _clampXmax = screenBounds.max.x;
        }
        // SINON, le dépassement maximal est de 0.5f
        // Calculé à partir du point central (transform.position)
        else
        {
            _clampXmin = screenBounds.min.x - ((objectBounds.size.x / 2f) - 0.5f);
            _clampXmax = screenBounds.max.x + ((objectBounds.size.x / 2f) - 0.5f);
        }

        // SI la hauteur de l'objet est plus petite que 0.5f, le depassement maximal Y de l'objet est de la moitié de sa hauteur
        // (transform.position donne un point au CENTRE de l'objet)
        if (objectBounds.size.y < 0.5f)
        {
            _clampYmin = screenBounds.min.y;
            _clampYmax = screenBounds.max.y;
        }
        // SINON, le dépassement maximal est de 0.5f
        // Calculé à partir du point central (transform.position)
        else
        {
            _clampYmin = screenBounds.min.y - ((objectBounds.size.y / 2f) - 0.5f);
            _clampYmax = screenBounds.max.y + ((objectBounds.size.y / 2f) - 0.5f);
        }

        _clampXmin -= (objectBounds.center.x - transform.position.x);
        _clampXmax -= (objectBounds.center.x - transform.position.x);
        _clampYmin -= (objectBounds.center.y - transform.position.y);
        _clampYmax -= (objectBounds.center.y - transform.position.y);
    }

    private IEnumerator CheckMovement()
    {
        bool moving = false;
        bool eventFired = false;

        while (true)
        {
            Vector2 startPos = transform.position;
            yield return new WaitForSeconds(0.1f);
            Vector2 currentPos = transform.position;
            moving = Vector2.Distance(startPos, currentPos) > 0.25f;

            if (!eventFired && moving)
            {
                eventFired = true;
                if (movableObjectMoving != null)
                    movableObjectMoving(this);
            }
            
        }
    }
}

public enum ObjectType
{
    Marker,
    Document,
    Message,
    Contract,
    Other
}
