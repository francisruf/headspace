/* Manager qui garde la trace de tous les objets "interactable", et gère leurs interactions.
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
        MovableObject.movableObjectSelected += OnMovableObjectSelected;
    }

    // Unsubscription
    private void OnDisable()
    {
        InteractableObject.objectEnabled -= OnObjectEnabled;
        InteractableObject.objectDisabled -= OnObjectDisabled;
        MovableObject.movableObjectSelected -= OnMovableObjectSelected;
    }

    // TEMP : Envoyer cette fonctionnalité dans le playerStateMachine
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SelectObjectOnMouseDown();
        }
    }

    private void OnMovableObjectSelected(MovableObject obj)
    {
        Debug.Log("Object selected : " + obj.gameObject.name);
    }

    // Fonction qui sera appellée par le playerStateMachine
    private void SelectObjectOnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.up, Color.red, 5f);

        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, objectLayers);

        InteractableObject candidateObject = null;
        InteractableObject selectedObject = null;

        foreach (var hit in hitsInfo)
        {
            //Debug.Log("HIT : " + hit.collider.gameObject.name);
            candidateObject = hit.collider.GetComponent<InteractableObject>();

            if (candidateObject != null)
            {
                if (selectedObject == null)
                {
                    selectedObject = candidateObject;
                    //Debug.Log("1- Selected : " + selectedObject.gameObject.name);
                }
                else
                {
                    int sortingLayer = candidateObject.GetSortingLayer();
                    int sortingOrder = candidateObject.GetOrderInLayer();
                    //Debug.Log("Selected sorting layer : " + selectedObject.GetSortingLayer());
                    //Debug.Log("Selected sorting order : " + selectedObject.GetOrderInLayer());
                    //Debug.Log("Candidate sorting layer : " + candidateObject.GetSortingLayer());
                    //Debug.Log("Candidate sorting order : " + candidateObject.GetOrderInLayer());

                    if (sortingLayer > selectedObject.GetSortingLayer())
                    {
                        selectedObject = candidateObject;
                        //Debug.Log("2- Selected : " + selectedObject.gameObject.name);
                    }
                    else if (sortingLayer == selectedObject.GetSortingLayer())
                    {
                        if (sortingOrder > selectedObject.GetOrderInLayer())
                        {
                            selectedObject = candidateObject;
                            //Debug.Log("3- Selected : " + selectedObject.gameObject.name);
                        }
                    }
                }
            }
        }

        if (selectedObject != null)
        {
            AssignTopRenderingOrder(selectedObject);
            selectedObject.Select();
        }
    }

    // TODO : Increase performance of this crap. (?)
    private void AssignTopRenderingOrder(InteractableObject selectedObject)
    {
        // Expression très fancy. Hésitez pas à me demander et je vous expliquerait kessé ça mange en hiver.
        var objectsWithSameLayer = _allActiveObjects.Where(x => x.GetSortingLayer() == selectedObject.GetSortingLayer()).ToList();
        //Debug.Log("Objects in same layer : " + objectsWithSameLayer.Count);

        int maxOrderInLayer = 0;
        foreach (var obj in objectsWithSameLayer)
        {
            if (obj.GetOrderInLayer() > maxOrderInLayer)
            {
                maxOrderInLayer = obj.GetOrderInLayer();
            }
        }
        //Debug.Log("New max order : " + maxOrderInLayer);
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
