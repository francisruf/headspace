using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class postItCoordinates : MonoBehaviour
{

    private Camera cam;
    public TextMeshProUGUI postItText;
    public LayerMask layermask;


    
    void Start()
    {
        cam = Camera.main;
  
    }
    void OnRightClick() 
        
    {
        //Fonction qui va chercher la position de la sourie en coordonnées, la convertie en coordonnées Unity, 
        // et la reconvertie en coordonnées de grille du jeu


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, layermask);
        //lance un raycast pour detecter les markers

        if (hit.collider != null)
            //si raycast touche markers, affiche les coordonnees du marker sur le post it
        {
            Vector2 markerPos = hit.collider.gameObject.transform.position;

            markerPos = GridCoords.FromWorldToGrid(markerPos);


           
            postItText.text = markerPos.ToString();
            Debug.Log(gameObject.transform.name);
        }

        else
        {
            Vector3 mousePos = Input.mousePosition;

            mousePos.z = 0f;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

            Vector3 mouseGridPos = GridCoords.FromWorldToGrid(mouseWorldPos);

            Vector2 vector2 = new Vector2(mouseGridPos.x, mouseGridPos.y);
            //convertit en vector2 pour enlever la coordonnees z sur le post it

            //Debug.Log(mouseGridPos);

            postItText.text = vector2.ToString();

        }

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            OnRightClick();
        }
    }
}
