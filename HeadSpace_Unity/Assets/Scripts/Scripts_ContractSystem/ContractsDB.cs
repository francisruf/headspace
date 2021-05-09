using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContractsDB : MonoBehaviour
{
    [Header("Settings")]
    public bool allCaps;

    [Header("Client names")]
    public List<string> clientFirstNames = new List<string>();
    private List<string> remainingFirstNames = new List<string>();
    public List<string> clientLastNames = new List<string>();
    private List<string> remainingLastNames = new List<string>();

    [Header("Client sprites")]
    public Sprite defaultSprite;
    public List<Sprite> clientFaceSprites = new List<Sprite>();
    private List<Sprite> remainingFaceSprites = new List<Sprite>();

    private void OnEnable()
    {
        LevelManager.resetGame += OnGameReset;
    }

    private void OnDisable()
    {
        LevelManager.resetGame -= OnGameReset;
    }

    private void Awake()
    {
        foreach (var n in clientFirstNames)
            remainingFirstNames.Add(n);

        foreach (var n in clientLastNames)
            remainingLastNames.Add(n);

        foreach (var s in clientFaceSprites)
            remainingFaceSprites.Add(s);
    }

    private void OnGameReset()
    {
        ResetDB();
    }

    public string GetRandomClientFirstName()
    {
        string firstName = "";

        int firstCount = remainingFirstNames.Count;
        if (firstCount <= 0)
            return "Name";

        int randomFirstIndex = UnityEngine.Random.Range(0, firstCount);
        firstName = remainingFirstNames[randomFirstIndex];
        remainingFirstNames.RemoveAt(randomFirstIndex);

        if (firstCount - 1 == 0)
        {
            ResetFirstNameDB();
        }

        if (allCaps)
            return firstName.ToUpper();
        else
            return firstName;
    }

    public string GetRandomClientLastName()
    {
        string lastName = "";

        int lastCount = remainingLastNames.Count;
        if (lastCount <= 0)
            return "LastName";

        int randomLastIndex = UnityEngine.Random.Range(0, lastCount);
        lastName = remainingLastNames[randomLastIndex];
        remainingLastNames.RemoveAt(randomLastIndex);

        if (lastCount - 1 == 0)
        {
            ResetLastNameDB();
        }

        if (allCaps)
            return lastName.ToUpper();
        else
            return lastName;
    }

    public Sprite GetRandomFaceSprite()
    {
        int count = remainingFaceSprites.Count;
        if (count <= 0)
            return defaultSprite;

        int randomIndex = UnityEngine.Random.Range(0, count);
        Sprite spr = remainingFaceSprites[randomIndex];
        remainingFaceSprites.RemoveAt(randomIndex);

        if (count - 1 == 0)
        {
            ResetFaceDB();
        }

        return spr;
    }

    public void ResetDB()
    {
        remainingFirstNames.Clear();
        remainingLastNames.Clear();
        remainingFaceSprites.Clear();

        foreach (var n in clientFirstNames)
            remainingFirstNames.Add(n);

        foreach (var n in clientLastNames)
            remainingLastNames.Add(n);

        foreach (var s in clientFaceSprites)
            remainingFaceSprites.Add(s);
    }

    private void ResetFaceDB()
    {
        remainingFaceSprites.Clear();

        foreach (var s in clientFaceSprites)
            remainingFaceSprites.Add(s);
    }

    private void ResetFirstNameDB()
    {
        remainingFirstNames.Clear();

        foreach (var n in clientFirstNames)
            remainingFirstNames.Add(n);
    }

    private void ResetLastNameDB()
    { 
        remainingLastNames.Clear();

        foreach (var n in clientLastNames)
            remainingLastNames.Add(n);
    }
}
