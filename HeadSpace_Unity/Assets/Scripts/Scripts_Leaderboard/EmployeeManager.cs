using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmployeeManager : MonoBehaviour
{
    public static EmployeeManager instance;

    [Header("Employees")]
    public Employee player;
    public EmployeesPerPromotion employees;

    [Header("Points balancing")]
    public List<PointsPerLevel> pointsPerLevels;

    public List<Employee> SortedEmployees { get; private set; }

    private void OnEnable()
    {
        GameManager.levelEnded += AssignPoints;
        LevelManager.resetGame += OnGameReset;
    }

    private void OnDisable()
    {
        GameManager.levelEnded -= AssignPoints;
        LevelManager.resetGame -= OnGameReset;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        SortedEmployees = new List<Employee>();
        SortedEmployees.Add(employees.first);
        SortedEmployees.Add(employees.second);
        SortedEmployees.Add(employees.third);
        SortedEmployees.Add(player);
    }

    private void OnGameReset()
    {
        SortedEmployees.Clear();
        SortedEmployees = new List<Employee>();
        SortedEmployees.Add(employees.first);
        SortedEmployees.Add(employees.second);
        SortedEmployees.Add(employees.third);
        SortedEmployees.Add(player);

        foreach (var emp in SortedEmployees)
        {
            emp.ClearCredits();
        }
    }

    private void AssignPoints()
    {
        int level = GameManager.instance.CurrentDayInfo.day;

        if (level < pointsPerLevels.Count)
        {
            employees.first.AddCredits(pointsPerLevels[level].first);
            employees.second.AddCredits(pointsPerLevels[level].second);
            employees.third.AddCredits(pointsPerLevels[level].third);

            int totalCredits = RessourceManager.instance.TotalCredits;
            int lastSectorCredits = RessourceManager.instance.CurrentCredits;

            player.AddCredits(lastSectorCredits);

            SortedEmployees = new List<Employee>();
            SortedEmployees.Add(employees.first);
            SortedEmployees.Add(employees.second);
            SortedEmployees.Add(employees.third);
            SortedEmployees.Add(player);

            Employee t;
            for (int p = 0; p <= 4 - 2; p++)
            {
                for (int i = 0; i <= 4 - 2; i++)
                {
                    if (SortedEmployees[i].TotalCredits < SortedEmployees[i + 1].TotalCredits)
                    {
                        t = SortedEmployees[i + 1];
                        SortedEmployees[i + 1] = SortedEmployees[i];
                        SortedEmployees[i] = t;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct PointsPerLevel
    {
        public int first;
        public int second;
        public int third;
    }

    [System.Serializable]
    public struct EmployeesPerPromotion
    {
        public Employee first;
        public Employee second;
        public Employee third;
    }
}
