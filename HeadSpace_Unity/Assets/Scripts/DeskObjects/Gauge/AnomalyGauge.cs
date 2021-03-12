using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomalyGauge : MonoBehaviour
{
    public SpriteRenderer gaugeFormatRenderer;
    public SpriteRenderer gaugeFillRenderer;
    public float addPercent;
    private float _fillAmount;

    private int _currentSouls;
    private int _sectorTotalSouls;
    private float _gaugePercent;

    private Vector2 _gaugeFormatSize;

    private void OnEnable()
    {
        PlanetManager.planetsSpawned += AssignTotalSouls;
        Planet.soulsLost += OnSoulsLost;
    }

    private void OnDisable()
    {
        PlanetManager.planetsSpawned -= AssignTotalSouls;
        Planet.soulsLost -= OnSoulsLost;
    }

    private void AssignTotalSouls(List<Planet> allPlanets)
    {
        for (int i = 0; i < allPlanets.Count; i++)
        {
            _sectorTotalSouls += allPlanets[i].TotalSouls;
        }

        _currentSouls = _sectorTotalSouls;
        Debug.Log("SECTOR HAS " + _sectorTotalSouls + " SOULS.");

        UpdateGaugeValue();
        UpdateGauge();
    }

    private void Start()
    {
        _gaugeFormatSize = gaugeFormatRenderer.size;
    }


    //private void Update()
    //{                  //quand on pese sur space change le _fillAmount de la gauge
    //    if (Input.GetKeyDown("space"))
    //    {
    //        //AddToGauge(addPercent);
    //        Debug.Log("gauge");
    //    }
    //}

    private void OnSoulsLost(Planet targetPlanet, int soulsLost)
    {
        _currentSouls -= soulsLost;
        UpdateGaugeValue();
        UpdateGauge();
    }

    private void UpdateGaugeValue()
    {
        _gaugePercent = 100f - ((float)_currentSouls / _sectorTotalSouls * 100f);
        Debug.Log("CURRENT SOULS : " + _currentSouls);
        Debug.Log("TOTAL SOULS : " + _sectorTotalSouls);
        Debug.Log("CURRENT GAUGE % : " + _gaugePercent);
    }

    private void UpdateGauge()
    {


        if (gaugeFillRenderer != null)
        {
            gaugeFillRenderer.size = new Vector2(_gaugeFormatSize.x, _gaugePercent / 100f * _gaugeFormatSize.y);
        }
    }
}
