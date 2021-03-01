using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class postItCoordinates : MonoBehaviour
{

    private Camera cam;
    public TextMeshProUGUI postItText;
   


    
    void Start()
    {
        cam = Camera.main;
  
    }
    void OnRightClick() 
        //Fonction qui va chercher la position de la sourie en coordonnées, la convertie en coordonnées Unity, 
        // et la reconvertie en coordonnées de grille du jeu
    {
        Vector3 mousePos = Input.mousePosition;

        mousePos.z = 0f;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 mouseGridPos = GridCoords.FromWorldToGrid(mouseWorldPos);

        Vector2 vector2 = new Vector2(mouseGridPos.x, mouseGridPos.y); 
        //convertit en vector2 pour enlever la coordonnees z sur le post it

        Debug.Log(mouseGridPos);

        postItText.text = vector2.ToString();

        //to do : convert mouseGridPos to vector2

    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            OnRightClick();
        }
    }
}
