using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EmployeeSlot : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI morningTotalScoreText;
    public TextMeshProUGUI nightTotalScoreText;
    public TextMeshProUGUI lastSectorScoreText;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InitializeSlot(LevelTime time, Employee emp)
    {
        nameText.enabled = true;
        nameText.text = emp.EmployeeName;
        _spriteRenderer.sprite = emp.polaroidSprite;

        switch (time)
        {
            case LevelTime.DayStart:
                Debug.Log("HI");
                morningTotalScoreText.enabled = true;
                nightTotalScoreText.enabled = false;
                lastSectorScoreText.enabled = false;
                morningTotalScoreText.text = emp.TotalCredits.ToString();
                break;

            case LevelTime.NightEnd:
                morningTotalScoreText.enabled = false;
                nightTotalScoreText.enabled = true;
                lastSectorScoreText.enabled = true;
                nightTotalScoreText.text = emp.TotalCredits.ToString();
                lastSectorScoreText.text = "+" + emp.LastSectorCredits.ToString();
                break;

            default:
                break;
        }
    }
}
