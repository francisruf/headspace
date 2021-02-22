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

    // État de l'objet
    private bool _isSelected;

    // Position de la souris lorsque l'objet est cliqué
    private Vector2 _mouseOffset;

    // Valeurs minimales et maximales pour le déplacement de l'objet
    private float _clampXmin;
    private float _clampXmax;
    private float _clampYmin;
    private float _clampYmax;

    private void Update()
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
    public override void Select()
    {
        _isSelected = true;
        _mouseOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateMinMaxPosition();

        if (movableObjectSelected != null)
            movableObjectSelected(this);
    }

    // Fonction qui force l'objet à être déposé
    public override void Deselect()
    {
        _isSelected = false;
        if (movableObjectDeselected != null)
            movableObjectDeselected(this);
    }

    // Fonction qui désactive l'objet (fonctionnalité complète dans classe de base - InteractableObject)
    protected override void DisableObject()
    {
        base.DisableObject();

        if (_isSelected)
        {
            Deselect();
        }
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

        Bounds screenBounds = new Bounds(Camera.main.transform.position, new Vector3(horExtent * 2f, vertExtent * 2f, 0f));
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
    }
}
