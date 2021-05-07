using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTemplateDB : MonoBehaviour
{
    [Header("Settings")]
    public bool allCaps;

    [Header("Planet names")]
    public List<string> planetNames = new List<string>();
    private List<string> remainingNames = new List<string>();

    [Header("Planet sprites")]
    public PlanetSpriteMatch defaultSpriteMatch;
    public List<PlanetSpriteMatch> planetSprites = new List<PlanetSpriteMatch>();
    private List<PlanetSpriteMatch> remainingSprites = new List<PlanetSpriteMatch>();

    private void Awake()
    {
        foreach (var n in planetNames)
        {
            remainingNames.Add(n);
        }
        foreach (var s in planetSprites)
        {
            remainingSprites.Add(s);
        }
    }

    public string GetRandomPlanetName()
    {
        int count = remainingNames.Count;
        if (count <= 0)
            return "Planet";

        int randomIndex = UnityEngine.Random.Range(0, count);
        string planetName = remainingNames[randomIndex];
        remainingNames.RemoveAt(randomIndex);

        if (allCaps)
            return planetName.ToUpper();
        else
            return planetName;
    }

    public PlanetSpriteMatch GetRandomPlanetSpriteMatch()
    {
        int count = remainingSprites.Count;
        if (count <= 0)
            return defaultSpriteMatch;

        int randomIndex = UnityEngine.Random.Range(0, count);
        PlanetSpriteMatch spr = remainingSprites[randomIndex];
        remainingSprites.RemoveAt(randomIndex);

        if (count - 1 == 0)
        {
            foreach (var s in planetSprites)
            {
                remainingSprites.Add(s);
            }
        }
        return spr;
    }

    public void ResetDB()
    {
        remainingNames.Clear();
        remainingSprites.Clear();

        foreach (var n in planetNames)
        {
            remainingNames.Add(n);
        }
        foreach (var s in planetSprites)
        {
            remainingSprites.Add(s);
        }
    }
}

[System.Serializable]
public struct PlanetSpriteMatch
{
    public Sprite mapSprite;
    public Sprite contractSprite;
}
