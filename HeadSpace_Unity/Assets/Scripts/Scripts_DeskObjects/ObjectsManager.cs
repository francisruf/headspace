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

    private IEnumerator _currentClickRoutine;
    private Vector2 _lastMousePos;
    private Vector2 _currentMousePos = new Vector2();

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
        LevelManager.unloadingDone += OnSceneUnload;
    }

    // Unsubscription
    private void OnDisable()
    {
        InteractableObject.objectEnabled -= OnObjectEnabled;
        InteractableObject.objectDisabled -= OnObjectDisabled;
        MovableObject.movableObjectSelected -= OnMovableObjectSelected;
        LevelManager.unloadingDone -= OnSceneUnload;
    }

    private void OnSceneUnload()
    {
        _allActiveObjects.Clear();
    }

    // TEMP : Envoyer cette fonctionnalité dans le playerStateMachine
    private void Update()
    {
        _lastMousePos = _currentMousePos;
        _currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        

        if (Input.GetMouseButtonDown(0))
        {
            SelectObjectOnMouseDown();
        }
    }

    private void LateUpdate()
    {
    }

    private void OnMovableObjectSelected(MovableObject obj)
    {
        //Debug.Log("Object selected : " + obj.gameObject.name);
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
            //Debug.Log("HIT : " + hit.collider.gameObject.name);
            candidateObject = hit.collider.GetComponent<InteractableObject>();

            if (candidateObject != null)
            {
                if (selectedObject == null)
                {
                    selectedObject = candidateObject;
                    int sortingLayer = candidateObject.GetSortingLayer();
                    int sortingOrder = candidateObject.GetOrderInLayer();
                    //Debug.Log("Selected sorting layer : " + selectedObject.GetSortingLayer());
                    //Debug.Log("Selected sorting order : " + selectedObject.GetOrderInLayer());
                    //Debug.Log("Candidate sorting layer : " + candidateObject.GetSortingLayer());
                    //Debug.Log("Candidate sorting order : " + candidateObject.GetOrderInLayer());
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
            if (_currentClickRoutine != null)
            {
                StopCoroutine(_currentClickRoutine);
            }
            _currentClickRoutine = AssignInteractionType(selectedObject);
            StartCoroutine(_currentClickRoutine);
        }
    }

    private IEnumerator AssignInteractionType(InteractableObject obj)
    {

        ObjectInteractionZone[] candidateInteractionZones = null;
        candidateInteractionZones = obj.GetInteractionZones();
        int interactionZonesCount = candidateInteractionZones.Length;
        bool dragged = false;

        if (interactionZonesCount <= 0)
        {
            AssignTopRenderingOrder(obj);
            obj.Select();
            _currentClickRoutine = null;
            yield return null;
        }
        else
        {
            float time = 0.0f;
            float maxTime = 0.3f;
            float mouseDelta = 0.0f;

            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool simpleClick = false;

            while (time < maxTime)
            {
                mouseDelta += Vector2.Distance(_currentMousePos, _lastMousePos);

                if (mouseDelta > 0.1f)
                {
                    AssignTopRenderingOrder(obj);
                    obj.Select();
                    dragged = true;
                    break;
                }

                yield return new WaitForEndOfFrame();

                time += Time.unscaledDeltaTime;

                if (!Input.GetMouseButton(0))
                {
                    simpleClick = true;
                    break;
                }
            }
            if (!dragged)
            {
                if (simpleClick)
                {
                    //Debug.Log("CLICK");
                    for (int i = 0; i < interactionZonesCount; i++)
                    {
                        if (candidateInteractionZones[i].IsInBounds(currentMousePos))
                        {
                            candidateInteractionZones[i].Interact();
                            break;
                        }
                    }
                }
                else
                {
                    AssignTopRenderingOrder(obj);
                    obj.Select();
                }
            }
        }
        _currentClickRoutine = null;
    }

    public void ForceTopRenderingOrder(InteractableObject obj)
    {
        AssignTopRenderingOrder(obj);
    }

    // TODO : Increase performance of this crap. (?)
    private void AssignTopRenderingOrder(InteractableObject selectedObject)
    {
        // Arrêter l'exécution s'il n'est pas souhaitable pour un objet d'être "bring to front"
        if (selectedObject.ignoreSelectedBringToFront)
            return;

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
