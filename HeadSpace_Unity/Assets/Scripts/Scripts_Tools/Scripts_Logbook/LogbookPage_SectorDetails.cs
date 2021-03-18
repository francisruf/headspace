using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogbookPage_SectorDetails : LogbookPage
{
    [Header("Sector page settings")]
    public GameObject pageParent;
    public TextMeshProUGUI soulsSavedAmountText;
    public TextMeshProUGUI planetsFoundAmountText;
    public TextMeshProUGUI entirePopulationsSavedAmountText;
    public TextMeshProUGUI creditsGainedAmountText;

    private int _soulsSaved;
    private int _planetsFound;
    private int _entirePopulationsSaved;
    private int _creditsGained;

    public override void DisplayPage(out Sprite newSprite)
    {
        base.DisplayPage(out newSprite);
        pageParent.SetActive(true);
    }

    public override void HidePage()
    {
        base.HidePage();
        pageParent.SetActive(false);
    }

    private void OnEnable()
    {
        SectorManager.sectorInfoUpdate += OnSectorInfoUpdate;
    }

    private void OnDisable()
    {
        SectorManager.sectorInfoUpdate -= OnSectorInfoUpdate;
    }

    private void OnSectorInfoUpdate(SectorInfo info)
    {
        _soulsSaved = info.SectorSoulsSaved;
        _planetsFound = info.PlanetsFound;
        _entirePopulationsSaved = info.PlanetsFullySaved;
        _creditsGained = info.CreditsGained;

        UpdateTexts();
    }

    private void UpdateTexts()
    {
        soulsSavedAmountText.text = _soulsSaved.ToString();
        planetsFoundAmountText.text = _planetsFound.ToString();
        entirePopulationsSavedAmountText.text = _entirePopulationsSaved.ToString();
        creditsGainedAmountText.text = _creditsGained.ToString();
    }

}
