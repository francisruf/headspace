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
        }

        if (col.GetComponent<DeployPoint>() != null) {
            ship.deployP = col.GetComponent<DeployPoint>();
            ship.isInDeployPoint = true;
        }

        if (col.GetComponent<Cloud>() != null){
            ship.isMoving = false;
            ship.isInCloud = true;
            mM.ContactWithCloudNotif(ship);
        }

        // Wormhole
        WormHole candidateWormhole = col.GetComponent<WormHole>();

        if (candidateWormhole != null)
        {
            // If collision with wormhole is NOT collision with the current RECEIVING wormhole
            if (candidateWormhole != ship.receiverWormhole)
            {
                // Assign references and call the ship function
                ship.senderWormhole = col.GetComponent<WormHole>();
                ship.receiverWormhole = ship.senderWormhole.SisterWormHole;
                ship.EnterWormHole(ship.receiverWormhole);
                mM.ContactWithWormholeNotif(ship);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col) {

        if (col.GetComponent<Planet_InteractionZone>() != null) {

            if (col.GetComponent<Planet_InteractionZone>().ParentPlanet == ship.planetInOrbit) {
                ship.isInPlanetOrbit = false;
                ship.planetInOrbit = null;
            }
        }

        if (col.GetComponent<DeployPoint>() != null) {

            if(col.GetComponent<DeployPoint>() == ship.deployP) {
                ship.isInDeployPoint = false;
                ship.deployP = null;
            }
        }

        if (col.GetComponent<Cloud>() != null) {
            ship.isInCloud = false;
        }

        // Wormhole
        WormHole candidateWormhole = col.GetComponent<WormHole>();

        if (candidateWormhole != null)
        {
            // If exiting the sender wormhole, clear the reference
            if (candidateWormhole == ship.senderWormhole) {
                ship.senderWormhole = null;
            }


            // If exiting the receiving wormhole, clear the reference
            else if (candidateWormhole == ship.receiverWormhole)
            {
                ship.receiverWormhole = null;
                ship.isInWormHole = false;
            }
        }
    }

}