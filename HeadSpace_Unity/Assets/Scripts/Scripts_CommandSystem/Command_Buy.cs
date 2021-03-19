using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command_Buy : Command
{
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
        int parsedCode = 0;
        bool validCode = int.TryParse(productCode, out parsedCode);
        errorMessage = "";

        if (!validCode)
        {
            return false;
        }

        //Validation 1 : -Vérifier avec le ShipManager si le vaisseau entré est trouvable.
        if (ShopManager.instance != null)
        {
            success = ShopManager.instance.TryBuyRequest(parsedCode);
        }
        else
        {
            Debug.Log("Error : Could not find ShopManager.");
        }
        return success;
    }
}
