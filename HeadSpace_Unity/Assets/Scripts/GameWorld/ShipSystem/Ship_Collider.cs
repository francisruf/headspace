using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Collider : MonoBehaviour {

    private MessageManager mM;
    private Ship ship;

    private void Awake() {
        //Besoin du prefab MessageManager dans la scene sinon ca cree des erreurs
        mM = MessageManager.instance;
        ship = GetComponentInParent<Ship>();
    }

    private void OnTriggerEnter2D(Collider2D col) {

        //When inside anomaly affected sector
        if (col.GetComponent<GridTile_Anomaly>() != null) {
            GridTile_Anomaly anomaly = col.GetComponent<GridTile_Anomaly>();

            mM.ContactWithAnomalyNotif(ship, anomaly);
        }

        if (col.GetComponent<DeployPoint>() != null && !ship.isMoving) {
            DeployPoint deploy = col.GetComponent<DeployPoint>();

            mM.EnteredDeployZoneNotif(ship, deploy);
        }
    }

    //private void OnTriggerStay2D(Collider2D col) {

    //    //When inside anomaly affected sector
    //    if (col.GetComponent<DeployPoint>() != null && !ship.isMoving) {
    //        DeployPoint deploy = col.GetComponent<DeployPoint>();

    //        mM.EnteredDeployZoneNotif(ship, deploy);
    //    }
    //}
}
