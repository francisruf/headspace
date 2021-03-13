using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Planet Archetype", menuName = "CustomObjects/Planet Archetype")]
public class PlanetArchetype : ScriptableObject
{
    // Paramètres de l'archétype
    public string archetypeName;
    public int minPopulation;
    public int maxPopulation;
    public int completionCreditsBonus;

    // Fonction qui retourne automatiquement une population entre le min et le max défini en paramètres.
    public int GetRandomPopulation()
    {
        return Random.Range(minPopulation, maxPopulation + 1);
    }

    public PlanetArchetype(string archetypeName, int minPopulation, int maxPopulation, int completionCreditsBonus)
    {
        this.archetypeName = archetypeName;
        this.minPopulation = minPopulation;
        this.maxPopulation = maxPopulation;
        this.completionCreditsBonus = completionCreditsBonus;
    }
}
