using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatsTracker : MonoBehaviour
{
    public Transform layoutParent;
    public GameObject statsTextPrefab;

    void Start()
    {
        if (LevelManager.instance == null)
            return;

        List<SectorInfo> allInfo = LevelManager.instance.AllSectorInfo;

        foreach (var info in allInfo)
        {
            TextMeshProUGUI text = Instantiate(statsTextPrefab, layoutParent).GetComponent<TextMeshProUGUI>();
            SetText(info, text);
        }
    }

    private void SetText(SectorInfo info, TextMeshProUGUI text)
    {
        string fullText = "Day : " + info.DayIndex;
        fullText += "\nGame time : " + info.GameTime;
        if (info.DayIndex == 0)
            fullText += "\nMerit points gained : -";
        else
            fullText += "\nMerit points gained : " + info.CreditsGained + " (max = " + info.TotalContracts * 10 + ")";
        fullText += "\nContracts completed : " + info.ContractsCompleted;
        fullText += "\nAverage time per contract : " + info.totalContractTimes / info.ContractsCompleted;
        fullText += "\n10 points contracts : " + info.tenPointsContracts;
        fullText += "\n8 points contracts : " + info.eightPointsContracts;
        fullText += "\n6 points contracts : " + info.sixPointsContracts;
        fullText += "\n1 points contracts : " + info.onePointsContracts;

        text.text = fullText;
    }
}
