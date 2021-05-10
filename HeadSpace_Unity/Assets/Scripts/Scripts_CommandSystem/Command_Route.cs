using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Route : Command
{
    public override bool TryExecution(string playerText, out string errorMessage)
    {
        errorMessage = "";
        return false;
    }

    public override bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage)
    {
        errorMessage = "";
        return false;
    }

    public override bool TryExecution(string shipName, List<string> route, out string errorMessage)
    {
        // Variables locales temporaires
        bool success = false;
        bool validShip = false;
        bool validCoords = false;
        string coordsError = "";
        Ship targetShip = null;
        errorMessage = "";

        //Validation 1 : -Vérifier avec le ShipManager si le vaisseau entré est trouvable.
        if (ShipManager.instance != null)
        {
            validShip = ShipManager.instance.FindShipByCallsign(shipName, out targetShip);
        }


        foreach (var pos in route)
        {
            TileCoordinates coords = default;
            validCoords = (GridCoords.FromTileNameToTilePosition(pos, out coords));
            if (!validCoords)
            {
                coordsError = "Position " + pos + " does not exist.";
                break;
            }
        }

        //TODO: Confirmation finale 1 : Le vaisseau est valide?
        if (!validShip)
        {
            success = false;
            errorMessage = "Invalid ship name : " + shipName;
        }
        else if (!validCoords)
        {
            errorMessage = "MOVE syntax error : \n\n" + coordsError;

            if (MessageManager.instance != null)
                MessageManager.instance.GenericMessage(errorMessage, true);

            success = false;
        }
        // Si tout est valide, exécution de la commande et envoie du bool succès au CommandManager
        else
        {
            success = true;
            ExecuteCommand(targetShip, route);
        }

        return success;
    }

    protected override void ExecuteCommand(Ship targetShip, List<string> route)
    {
        targetShip.StartNewRoute(route);
    }

}
