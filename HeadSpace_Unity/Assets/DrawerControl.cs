using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerControl : MonoBehaviour
{

    private float startPosX;
    private float startPosY;
    private float boxHalfHeight;
    private const float drawerHandleSize = 1f;
    private bool isBeingHeld = false;


    private void Start()
    {
       
        startPosX = transform.position.x;
        startPosY = transform.position.y;
        boxHalfHeight = GetComponent<SpriteRenderer>().bounds.size.y * 0.5f;
        
    }



    // Update is called once per frame
    void Update()
    {
        if(isBeingHeld)
        {
            Vector3 mousePos;

            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            gameObject.transform.position = new Vector3(transform.position.x, Mathf.Clamp(mousePos.y, Camera.main.orthographicSize - boxHalfHeight, Camera.main.orthographicSize + boxHalfHeight - drawerHandleSize), 0); 
        }
    }

    private void OnMouseUp()
    {
        isBeingHeld = false;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos;

            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);
            isBeingHeld = true;
        }

    }
}
