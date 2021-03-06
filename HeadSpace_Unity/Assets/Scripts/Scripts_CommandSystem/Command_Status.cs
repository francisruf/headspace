﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Status : Command
{
    public override bool TryExecution(string playerText, out string errorMessage)
    {
        bool success = false;
        errorMessage = "Move command not implemented.";
        return success;
    }

    // Fonction qui essaie d'exécuter la commande à partir de la syntaxe fournie.
    // Si la syntaxe est invalide, la commande ne s'exécute pas, et renvoie un message d'erreur au CommandManager.
    public override bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage)
    {
        // Variables locales temporaires
        bool success = false;
        bool validShip = true;
        Ship targetShip = null;
        errorMessage = "";

        //Validation 1 : -Vérifier avec le ShipManager si le vaisseau entré est trouvable.
        if (ShipManager.instance != null)
        {
            validShip = ShipManager.instance.FindShipByCallsign(shipName, out targetShip);
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
        targetShip.Status();
    }
}
