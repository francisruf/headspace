using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Une classe abstraite est une classe générale qui ne peut être directement appliquée à un objet.
// Elle sert principalement à déterminer des fonctionnalités partagées par plusieurs SOUS-CLASSES, qui héritent de celle-ci.
public abstract class InteractableObject : MonoBehaviour
{
    public static Action<InteractableObject> objectEnabled;
    public static Action<InteractableObject> objectDisabled;

    // Protected = Comme private, mais accessible par les classes qui HÉRITENT de cette classe
    protected SpriteRenderer _spriteRenderer;
    protected Collider2D _collider;
    public Bounds ColliderBounds { get { return _collider.bounds; } }
    public SpriteRenderer ObjSpriteRenderer {get { return _spriteRenderer; }}

    // SpriteRenderers des child objects
    protected SpriteRenderer[] _childSpriteRenderers;
    protected int _childSpriteRenderersCount;
    // Canvas des child objects
    protected Canvas[] _childCanvases;
    protected int _childCanvasesCount;

    protected ObjectInteractionZone[] _interactionZones;

    // État de l'objet
    protected bool _isSelected;

    public int CurrentSortingLayer { get; set; }

    [Header("Interactable object settings")]
    public bool ignoreSelectedBringToFront;

    // Virtual = Une classe qui HÉRITE de InteractableObject peut REMPLACER ou MODIFIER la fonction Awake à sa façon
    protected virtual void Awake()
    {
        // Assigner les références de components
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        _childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _childCanvases = GetComponentsInChildren<Canvas>();
        _interactionZones = GetComponentsInChildren<ObjectInteractionZone>();

        _childSpriteRenderersCount = _childSpriteRenderers.Length;
        _childCanvasesCount = _childCanvases.Length;
    }

    protected virtual void Start()
    {
        InitializeObject();
    }

    // Fonction ABSTRACT : Une fonction qui doit être définie par chacune des classes qui HÉRITENT de celle-ci
    public virtual void Select()
    {
        _isSelected = true;
        
        if (!ignoreSelectedBringToFront)
        {
            CurrentSortingLayer = _spriteRenderer.sortingLayerID;

            SetSortingLayer(SortingLayer.NameToID("SelectedObject"));
        }
    }
    public virtual void Deselect()
    {
        _isSelected = false;

        if (!ignoreSelectedBringToFront)
        {
            SetSortingLayer(CurrentSortingLayer);
        }
    }

    // Fonction qui envoie l'objet au manager lorsqu'il est initialisé
    protected virtual void InitializeObject()
    {
        if (objectEnabled != null)
            objectEnabled(this);
    }

    // Fonction qui communique au manager que l'objet est désactivé
    // Et qui désactive ses components
    public virtual void DisableObject()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.enabled = false;

        if (_collider != null)
            _collider.enabled = false;

        if (objectDisabled != null)
            objectDisabled(this);
    }

    // Fonction qui retourne le sorting layer en INT d'un sprite renderer,
    // pour déterminer lequel est sélectionné selon sa priorité de rendering
    public virtual int GetSortingLayer()
    {
        // Si l'objet a bel et bien un sprite renderer
        if (_spriteRenderer != null)
        {
            return SortingLayer.GetLayerValueFromID(_spriteRenderer.sortingLayerID);
        }
        // Sinon, retourner 0 (ne devrait pas arriver)
        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
            return 0;
        }
    }

    public virtual void SetSortingLayer(int newSortingLayerID)
    {
        // Si l'objet a bel et bien un sprite renderer
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingLayerID = newSortingLayerID;

            for (int i = 0; i < _childSpriteRenderersCount; i++)
            {
                if (_childSpriteRenderers[i] != _spriteRenderer)
                {
                    _childSpriteRenderers[i].sortingLayerID = _spriteRenderer.sortingLayerID;
                }
            }

            for (int i = 0; i < _childCanvasesCount; i++)
            {
                _childCanvases[i].sortingLayerID = _spriteRenderer.sortingLayerID;
            }
        }

        // Sinon, retourner 0 (ne devrait pas arriver)
        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
        }
    }

    // Fonction qui retourne le order in layer en INT d'un sprite renderer.
    // Suite de la fonctionnalité précédente
    public virtual int GetOrderInLayer()
    {
        // Si l'objet a bel et bien un sprite renderer
        if (_spriteRenderer != null)
        {
            return _spriteRenderer.sortingOrder;
        }
        // Sinon, retourner 0 (ne devrait pas arriver)
        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
            return 0;
        }
    }

    public virtual void SetOrderInLayer(int newOrderInLayer)
    {
        // Si l'objet a bel et bien un sprite renderer
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = newOrderInLayer;

            int currentOrder = _spriteRenderer.sortingOrder + 1;
            for (int i = 0; i < _childSpriteRenderersCount; i++)
            {
                if (_childSpriteRenderers[i] != _spriteRenderer)
                {
                    _childSpriteRenderers[i].sortingOrder = currentOrder;
                    currentOrder++;
                }
            }

            for (int i = 0; i < _childCanvasesCount; i++)
            {
                _childCanvases[i].sortingOrder = currentOrder;
                currentOrder++;
            }
        }
        // Sinon, retourner 0 (ne devrait pas arriver)
        else
        {
            Debug.LogError("Warning : No spriteRenderer found on " + gameObject.name);
        }
    }

    public virtual ObjectInteractionZone[] GetInteractionZones()
    {
        return _interactionZones;
    }

}
