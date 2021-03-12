using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Collider : MonoBehaviour {

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

        //When inside anomaly affected sector
        if (col.GetComponent<GridTile_Anomaly>() != null) {
            GridTile_Anomaly anomaly = col.GetComponent<GridTile_Anomaly>();

            mM.ContactWithAnomalyNotif(ship, anomaly);
        }

        if (col.GetComponent<Planet_InteractionZone>() != null) {
            ship.planetInOrbit = col.GetComponent<Planet_InteractionZone>().ParentPlanet;
            ship.isInPlanetOrbit = true;
            Debug.Log("Ca EnterTrigger comme du monde!");
        }

        if (col.GetComponent<DeployPoint>() != null) {
            ship.deployP = col.GetComponent<DeployPoint>();
            ship.isInDeployPoint = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col) {

        if (col.GetComponent<Planet_InteractionZone>() != null) {

            if (col.GetComponent<Planet_InteractionZone>().ParentPlanet == ship.planetInOrbit) {
                ship.isInPlanetOrbit = false;
                ship.planetInOrbit = null;
                //ship.isLoadingSouls = false;
                Debug.Log("Ca ExitTrigger comme du monde!");
            }
        }

        if (col.GetComponent<DeployPoint>() != null) {

            if(col.GetComponent<DeployPoint>() == ship.deployP) {
                ship.isInDeployPoint = false;
                ship.deployP = null;
            }
        }
    }

}