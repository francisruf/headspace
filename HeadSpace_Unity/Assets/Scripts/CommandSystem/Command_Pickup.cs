using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Pickup : Command
{
    // FUTURE VARIANTE DE FONCTION À IMPLÉMENTER, PAS BESOIN D'Y TOUCHER POUR L'INSTANT
    public override bool TryExecution(string playerText, out string errorMessage)
    {
        bool success = false;
        errorMessage = "Move command not implemented.";

        /* TODO :
         * - Decode full text
         * - Find ship from ShipManager
         * - Verify if coordinates are valid
         * - Send Command
         */

        return success;
    }

    // Fonction qui essaie d'exécuter la commande à partir de la syntaxe fournie.
    // Si la syntaxe est invalide, la commande ne s'exécute pas, et renvoie un message d'erreur au CommandManager.
    public override bool TryExecution(string shipName, string coordinatesText, out string errorMessage)
    {
        // Variables locales temporaires
        bool success = false;
        bool validShip = true;
        Ship targetShip = null;
        errorMessage = "";

        //Validation 1 : -Vérifier avec le ShipManager si le vaisseau entré est trouvable.
        if (ShipManager.instance != null)
        {
            validShip = ShipManager.instance.FindShipByName(shipName, out targetShip);
        }
        else
        {
            Debug.Log("Error : Could not find ShipManager.");
        }

        //TODO: Confirmation finale 1 : Le vaisseau est valide?
        if (!validShip)
        {
            success = false;
            errorMessage = "Invalid ship name : " + shipName;
        }

        // Si tout est valide, exécution de la commande et envoie du bool succès au CommandManager
        else
        {
            success = true;
            ExecuteCommand(targetShip);
        }

        return success;
    }

    protected override void ExecuteCommand(Ship targetShip)
    {
        // La validation de la commande (si le vaisseau se trouve dans la zone d'interaction de la planète ou non)
        // devrait se faire dans le script du vaisseau lui-même
        Debug.Log("PICKUP CALLED");
    }
}
