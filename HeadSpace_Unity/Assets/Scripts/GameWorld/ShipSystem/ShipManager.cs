using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Classe à continuer.
 * J'ai seulement commencé pour pouvoir implémenter les bonnes références dans le CommandManager
 * 
 * Le ShipManager sert à conserver une référence à tous les Ship actifs, afin de dispatcher l'information au besoin :
 * -- Pour le UI d'inventaire
 * -- Pour la gestion des commandes
 * 
 * Pistes : 
 * - Utiliser une Liste qui se remplit à chaque vaisseau qui s'active
 * - Utiliser une fonction de recherche de vaisseau dans cette liste, à l'aide du NOM d'un vaisseau (un string public)
 */

public class ShipManager : MonoBehaviour
{
    // Singleton
    public static ShipManager instance;

    private void Awake()
    {
        // Déclaration du singleton
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    /* Fonction qui retourne VRAI ou FAUX si un vaisseau a été trouvé.
     * La fonction assigne également un paramètre de SORTIE avec la clause OUT, permettant d'envoyer
     * une référence au vaisseau trouvé du même coup.
     * 
     * N'hésite pas à m'écrire si tu veux plus d'explications :)
     */
    public bool FindShipByName(string shipNameToFind, out Ship foundShip)
    {
        bool shipNameFound = false;
        foundShip = null;

        return shipNameFound;
    }
}
