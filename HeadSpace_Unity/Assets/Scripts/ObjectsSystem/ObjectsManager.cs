﻿/* Manager qui garde la trace de tous les objets "interactable", et gère leurs interactions.
 * Fonctionnalités principales : 
 * -- Gérer la sélection des objets par ordre de visibilité
 * -- Ordonner le rendering des objets (un objet sélectionné est rendered par dessus les autres)
 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsManager : MonoBehaviour
{
    // Singleton
    public static ObjectsManager instance;

    // Layer mask : Détermine quels layers peuvent être touchés par un raycast OnMouseClick
    public LayerMask objectLayers;

    // Liste de tous les objets actifs dans la scène
    private List<InteractableObject> _allActiveObjects = new List<InteractableObject>();

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    // Subscription aux Actions des autres classes
    private void OnEnable()
    {
        InteractableObject.objectEnabled += OnObjectEnabled;
        InteractableObject.objectDisabled += OnObjectDisabled;
    }

    // Unsubscription
    private void OnDisable()
    {
        InteractableObject.objectEnabled -= OnObjectEnabled;
        InteractableObject.objectDisabled -= OnObjectDisabled;
    }

    // TEMP : Envoyer cette fonctionnalité dans le playerStateMachine
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectObjectOnMouseDown();
        }
    }

    // Fonction qui sera appellée par le playerStateMachine
    private void SelectObjectOnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, objectLayers);

        InteractableObject candidateObject = null;
        InteractableObject selectedObject = null;

        foreach (var hit in hitsInfo)
        {
            candidateObject = hit.collider.GetComponent<InteractableObject>();

            if (candidateObject != null)
            {
                if (selectedObject == null)
                {
                    selectedObject = candidateObject;
                }
                else
                {
                    int sortingLayer = candidateObject.GetSortingLayer();
                    int sortingOrder = candidateObject.GetOrderInLayer();

                    if (sortingLayer > selectedObject.GetSortingLayer())
                    {
                        selectedObject = candidateObject;
                    }
                    else if (sortingLayer == selectedObject.GetSortingLayer())
                    {
                        if (sortingOrder > selectedObject.GetOrderInLayer())
                        {
                            selectedObject = candidateObject;
                        }
                    }
                }
            }
        }

        if (selectedObject != null)
        {
            selectedObject.Select();
            AssignTopRenderingOrder(selectedObject);
        }
    }

    // TODO : Increase performance of this crap. (?)
    private void AssignTopRenderingOrder(InteractableObject selectedObject)
    {
        // Expression très fancy. Hésitez pas à me demander et je vous expliquerait kessé ça mange en hiver.
        var objectsWithSameLayer = _allActiveObjects.Where(x => x.GetSortingLayer() == selectedObject.GetSortingLayer()).ToList();

        int maxOrderInLayer = 0;
        foreach (var obj in objectsWithSameLayer)
        {
            if (obj.GetOrderInLayer() > maxOrderInLayer)
            {
                maxOrderInLayer = obj.GetOrderInLayer();
            }
        }
        selectedObject.SetOrderInLayer(maxOrderInLayer + 1);

    }

    // Fonction appelée à l'aide d'une ACTION dans la classe interactableObject, lorsqu'un objet est activé
    private void OnObjectEnabled(InteractableObject obj)
    {
        // Ajout du nouvel objet à la liste
        _allActiveObjects.Add(obj);
    }

    // Fonction appelée à l'aide d'une ACTION dans la classe interactableObject, lorsqu'un objet est désactivé
    private void OnObjectDisabled(InteractableObject obj)
    {
        // Enlever l'objet de la liste
        _allActiveObjects.Remove(obj);
    }
}
