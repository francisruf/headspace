/* Classe générique de commande
 * - Classe ABSTRACT : Une classe abstraite ne peut être instanciée, et donc déposée sur un script directement.
 * - La classe abstraite sert à définir des fonctionnalités qui pourront être partagées par
 * - des objets qui HÉRITENT de cette classe
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Command : MonoBehaviour
{
    // Nom de la commande, à entrer par le joueur
    public string keyWord;

    // Fonctions à définir dans les classes qui héritent de Command
    // Le CommandManager appelle toujours cette fonction et envoie les paramètres STRING
    public abstract bool TryExecution(string playerText, out string errorMessage);
    public abstract bool TryExecution(string shipName, string coordinatesText, string productCode, out string errorMessage);

    // Fonctions vides qui peuvent être override (remplacées) dans les classes qui héritent de Command
    protected virtual void ExecuteCommand()
    {
    }

    protected virtual void ExecuteCommand(Ship targetShip)
    {
    }

    protected virtual void ExecuteCommand(Ship targetShip, Vector2 gridCoordinates)
    {
    }

    /* Fonction qui essaie de convertir des coordonnées écrites en coordonnées Vector2 : 
     * - Validation de la syntaxe
     * - Validation avec les GridCoords si la grille contient les coordonnées
     * - Retourne TRUE/FALSE et les coordonnées en paramètre de SORTIE (out)
     */
    protected bool TryParseCoordinates(string coordinatesText, out Vector2 parsedCoords)
    {
        parsedCoords = Vector2.zero;
        string xCoordsText = "";
        string yCoordsText = "";

        int charCount;

        for (charCount = 0; charCount < coordinatesText.Length; charCount++)
        {
            bool foundSeparator = false;

            if (coordinatesText[charCount] == '(')
                charCount++;

            if (coordinatesText[charCount] == '/')
                foundSeparator = true;

            else if (coordinatesText[charCount] == ',')
                foundSeparator = true;
            
            else if (coordinatesText[charCount] == '-')
                foundSeparator = true;

            if (foundSeparator)
            {
                break;
            }
            else
            {
                xCoordsText += coordinatesText[charCount];
            }
        }

        float xCoords;
        bool xIsValid = float.TryParse(xCoordsText, out xCoords);

        if (!xIsValid)
        {
            return false;
        }

        for (charCount = charCount + 1; charCount < coordinatesText.Length; charCount++)
        {
            if (coordinatesText[charCount] == ')')
                break;

            yCoordsText += coordinatesText[charCount];
        }

        float yCoords;
        bool yIsValid = float.TryParse(yCoordsText, out yCoords);

        if (!yIsValid)
        {
            return false;
        }

        bool validGridCoords;
        parsedCoords = new Vector2(xCoords, yCoords);
        parsedCoords = GridCoords.RoundCoords(parsedCoords, out validGridCoords);

        if (!validGridCoords)
        {
            return false;
        }
        else
        {
            //Debug.Log("Successfully parsed coords : " + parsedCoords);
        }

        return true;
    }
}
