using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New employee", menuName = "Characters/Employee")]
public class Employee : ScriptableObject
{
    public CharacterName employeeName;
    public Sprite polaroidSprite;
    public int TotalCredits { get; private set; }
    public int LastSectorCredits { get; private set; }

    public void AddCredits(int credits)
    {
        TotalCredits += credits;
        LastSectorCredits = credits;
    }

    public string EmployeeName
    {
        get
        {
            if (employeeName == CharacterName.Player)
                return "You";
            else
                return employeeName.ToString();
        }
    }
}

public enum CharacterName
{
    Player,
    Billy,
    Julie,
    Henry
}
