using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyGauge : MonoBehaviour
{
    public SpriteRenderer gaugeFormatRenderer;
    public SpriteRenderer gaugeFillRenderer;
    private float _fillAmount;

    private int _currentSouls;
    private int _sectorTotalSouls;
    private float _gaugePercent;

    private Vector2 _gaugeFormatSize;

    private void OnEnable()
    {
        PlanetManager.planetsSpawned += AssignTotalSouls;
        Planet.soulsLost += OnSoulsLost;
        GridManager.gridDataDestroyed += OnGridDataDestroyed;
    }

    private void OnDisable()
    {
        PlanetManager.planetsSpawned -= AssignTotalSouls;
        Planet.soulsLost -= OnSoulsLost;
        GridManager.gridDataDestroyed -= OnGridDataDestroyed;
    }

    private void AssignTotalSouls(List<Planet> allPlanets)
    {
        for (int i = 0; i < allPlanets.Count; i++)
        {
            _sectorTotalSouls += allPlanets[i].TotalSouls;
        }

        _currentSouls = _sectorTotalSouls;
        //Debug.Log("SECTOR HAS " + _sectorTotalSouls + " SOULS.");

        UpdateGaugeValue();
    }

    private void Start()
    {
        _gaugeFormatSize = gaugeFormatRenderer.size;
    }

    private void OnGridDataDestroyed()
    {
        _currentSouls = 0;
        _sectorTotalSouls = 0;
        UpdateGaugeValue();
    }

    private void OnSoulsLost(Planet targetPlanet, int soulsLost)
    {
        _currentSouls -= soulsLost;
        UpdateGaugeValue();
    }

    private void UpdateGaugeValue()
    {
        _gaugePercent = 100f - ((float)_currentSouls / _sectorTotalSouls * 100f);
        UpdateGaugeVisuals();
    }

    private void UpdateGaugeVisuals()
    {

        if (gaugeFillRenderer != null)
        {
            gaugeFillRenderer.size = new Vector2(_gaugeFormatSize.x, _gaugePercent / 100f * _gaugeFormatSize.y);
        }
    }
}
