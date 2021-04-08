using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTemplateDB : MonoBehaviour
{
    [Header("Planet names")]
    public List<string> planetNames = new List<string>();
    private List<string> remainingNames = new List<string>();

    [Header("Planet sprites")]
    public Sprite defaultSprite;
    public List<Sprite> planetSprites = new List<Sprite>();
    private List<Sprite> remainingSprites = new List<Sprite>();

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
        return planetName;
    }

    public Sprite GetRandomPlanetSprite()
    {
        int count = remainingSprites.Count;
        if (count <= 0)
            return defaultSprite;

        int randomIndex = UnityEngine.Random.Range(0, count);
        Sprite spr = remainingSprites[randomIndex];
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
