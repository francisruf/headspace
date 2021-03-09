using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{


    public GameObject Drawer;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("1");
        if (collider.GetComponent<MovableMarker>() != null) 
        {
            collider.transform.SetParent(transform);
            Debug.Log("2");
        }
        
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
      

        if (collider.GetComponent<MovableMarker>() != null)
        {
            collider.transform.parent = null;
            Debug.Log("3");
        }
    }

}
