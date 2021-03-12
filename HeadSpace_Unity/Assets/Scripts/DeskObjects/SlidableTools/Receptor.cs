using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receptor : MonoBehaviour
{

    public GameObject messagePrefab;
    public Transform printPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) {
            PrintMessage();
        }
    }

    //Si le recepteur est sorti: Print message (Print next message in Queue plus tard)

    private void PrintMessage() {
        GameObject message = Instantiate(messagePrefab, printPoint.position, printPoint.rotation, printPoint);
    }
}
