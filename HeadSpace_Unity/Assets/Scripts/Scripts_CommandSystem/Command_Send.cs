using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Send : Command
{
    // FUTURE VARIANTE DE FONCTION À IMPLÉMENTER, PAS BESOIN D'Y TOUCHER POUR L'INSTANT
    public override bool TryExecution(string playerText, out string errorMessage)
    {
        errorMessage = "";
        return false;
    }

    // Fonction qui essaie d'exécuter la commande à partir de la syntaxe fournie.
    // Si la syntaxe est invalide, la commande ne s'exécute pas, et renvoie un message d'erreur au CommandManager.
    public override bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage)
    {
        // Variables locales temporaires
        bool success = false;
        bool validShip = false;
        bool validCoords = false;
        Ship targetShip = null;
        bool coordsInDeployPoint = false;
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

        // Validation 2 : Vérifier si les coordonnées sont valides, si oui les assigner à parsedCoords
        Vector2 parsedCoords;
        validCoords = TryParseCoordinates(coordinatesText, out parsedCoords);   // Fonction dans la classe de base

        // Validation 3 : Vérifier si les coordonnées se trouvent dans un point de déploiement.
        if (validCoords)
        {
            if (DeployManager.instance != null)
            {
                coordsInDeployPoint = DeployManager.instance.IsInDeployPoint(parsedCoords);
            }
            else
            {
                Debug.Log("Warning : No DeployManager could be found");
                coordsInDeployPoint = false;
            }
        }

        //Confirmation finale 1 : Le vaisseau est valide?
        if (!validShip)
        {
            success = false;
            errorMessage = "Invalid ship name : " + shipName;
        }

        // Confirmation finale 2 : Les coordonnées sont valides?
        else if (!validCoords)
        {
            success = false;
            errorMessage = "Invalid coordinates : " + coordinatesText;
        }

        // Confirmation finale 3 : Les coordonnées sont dans un point de déploiement?
        else if (!coordsInDeployPoint)
        {
            success = false;
            errorMessage = "Coordinates are outisde of Deploy Point : " + coordinatesText;
        }

        // Si tout est valide, exécution de la commande et envoie du bool succès au CommandManager
        else
        {
            success = true;
            ExecuteCommand(targetShip, parsedCoords);
        }

        return success;
    }

    protected override void ExecuteCommand(Ship targetShip, Vector2 gridCoordinates)
    {
        //Debug.Log("DEPLOY CALLED");
        targetShip.Deploy(gridCoordinates);
    }
}
