using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Detection_Radius : MonoBehaviour
{
    CircleCollider2D detectionCollider;
    public float detectionRadius;

    // Start is called before the first frame update
    void Start()
    {
        detectionCollider = GetComponent<CircleCollider2D>();
        detectionCollider.radius = detectionRadius;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.P)) {
        //    detectionRadius.radius += 1f;
        //}
        detectionCollider.radius = detectionRadius;
    }
}
