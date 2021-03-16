using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Detection_Radius : MonoBehaviour
{

    private MessageManager mM;
    private Ship ship;

    private void Awake() {
        ship = GetComponentInParent<Ship>();
    }

    private void Start() {
        //Besoin du prefab MessageManager dans la scene sinon ca cree des erreurs
        mM = MessageManager.instance;
    }

    private void OnTriggerEnter2D(Collider2D col) {

        //Detect planet
        if (col.GetComponent<Planet_ShipZone>() != null) {
            Planet planet = col.GetComponent<Planet_ShipZone>().ParentPlanet;

            mM.NewPlanetDetectedNotif(ship, planet);
        }

        //Detect transmission from a planet
        //Deactivated for now
        //if (col.GetComponent<Planet_TransmissionZone>() != null) {
        //    Planet planet = col.GetComponent<Planet_TransmissionZone>().ParentPlanet;

        //    mM.NewTransmissionDetectedNotif(ship, planet);
        //}

        //Detect anomly tile
        //Deactivated for now
        //if (col.GetComponent<GridTile_Anomaly>() != null) {
        //    GridTile_Anomaly anomaly = col.GetComponent<GridTile_Anomaly>();

        //    mM.NewAnomalyDetectedNotif(ship, anomaly);
        //}

        //Detects cloud or wormhole
        if (col.GetComponent<Hazard>() != null) {
            Hazard hazard = col.GetComponent<Hazard>();

            mM.NewHazardDetectedNotif(ship, hazard);
        }


    }
}