using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Detection_Radius : MonoBehaviour
{

    private MessageManager mM;
    private Ship ship;

    private void Awake() {
        //Besoin du prefab MessageManager dans la scene sinon ca cree des erreurs
        mM = MessageManager.instance;
        ship = GetComponentInParent<Ship>();
    }

    private void OnTriggerEnter2D(Collider2D col) {

        //Detect planet, wormhole, cloud
        if (col.GetComponent<GridStaticObject>() != null) {
            GridStaticObject obj = col.GetComponent<GridStaticObject>();

            mM.NewObjectDetectedNotif(ship, obj);
        }

        //Detect anomly tile
        if (col.GetComponent<GridTile_Anomaly>() != null) {
            GridTile_Anomaly anomaly = col.GetComponent<GridTile_Anomaly>();

            mM.NewAnomalyDetectedNotif(ship, anomaly);
        }

        //Detects a transmission coming from a planet
        if (col.gameObject.name == "TransmissionZone") {
            Debug.Log("I detected a Transmission coming from a planet!");
        }

    }
}