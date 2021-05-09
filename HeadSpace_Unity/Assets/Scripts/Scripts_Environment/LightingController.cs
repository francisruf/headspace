using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class LightingController : MonoBehaviour
{
    [Header("Animation settings")]
    public bool randomFlick;

    [Header("Lights")]
    public List<Light2D> targetLights;
    private List<LightData> _allLights;
    private Queue<IEnumerator> lightRoutineQueue = new Queue<IEnumerator>();
    private int _queueCount;
    private IEnumerator _currentLightRoutine;
    private IEnumerator _currentDisabledRoutine;

    public Light2D anomalyFocusLight;

    private void Awake()
    {
        _allLights = new List<LightData>();
        foreach (var light in targetLights)
        {
            _allLights.Add(new LightData(light, light.intensity));
        }
    }

    private void OnEnable()
    {
        GameManager.levelStarted += OnLevelStart;
        Ship.shipDisabled += OnShipDisabled;
    }

    private void OnDisable()
    {
        GameManager.levelStarted -= OnLevelStart;
        Ship.shipDisabled -= OnShipDisabled;
    }

    private void Update()
    {
        if (_queueCount > 0 && _currentLightRoutine == null)
        {
            _currentLightRoutine = lightRoutineQueue.Dequeue();
            _queueCount--;
            StartCoroutine(_currentLightRoutine);
        }
    }

    private void OnLevelStart()
    {
        if (randomFlick)
            StartCoroutine(RandomFlick());
    }

    private void OnShipDisabled(Ship ship)
    {
        if (anomalyFocusLight == null)
            return;

        Debug.Log("YO");

        if (_currentDisabledRoutine == null)
        {
            _currentDisabledRoutine = ShipDisableRoutine(ship);
            StartCoroutine(_currentDisabledRoutine);
        }
    }

    private IEnumerator ShipDisableRoutine(Ship ship)
    {
        NewLightAnimRequest(DimLights(0.4f, 0.4f));
        anomalyFocusLight.transform.position = ship.transform.position;

        int count = 0;
        while (count < 6)
        {
            count++;
            anomalyFocusLight.enabled = true;
            yield return new WaitForSeconds(0.5f);
            anomalyFocusLight.enabled = false;
            yield return new WaitForSeconds(0.3f);
        }
        NewLightAnimRequest(DimLights(1.0f, 1.0f));
        anomalyFocusLight.enabled = false;
        _currentDisabledRoutine = null;
    }

    private IEnumerator RandomFlick()
    {
        while (GameManager.GameStarted)
        {
            int randomTime = UnityEngine.Random.Range(30, 120);
            yield return new WaitForSeconds(randomTime);
            Flicker();
        }
    }

    public void Flicker()
    {
        NewLightAnimRequest(DimLights(0.85f, 0.08f));
        NewLightAnimRequest(DimLights(0.91f, 0.11f));
        NewLightAnimRequest(DimLights(0.78f, 0.07f));
        NewLightAnimRequest(DimLights(1.0f, 0.13f));
    }

    private void NewLightAnimRequest(IEnumerator routine)
    {
        if (_currentLightRoutine != null)
        {
            lightRoutineQueue.Enqueue(routine);
            _queueCount++;
        }
        else
        {
            _currentLightRoutine = routine;
            StartCoroutine(_currentLightRoutine);
        }
    }

    private IEnumerator DimLights(float targetRatio, float dimTime)
    {
        Debug.Log("START");
        float time = 0f;
        float timeStep = 1f / 60f;
        int lightCount = _allLights.Count;

        List<float> steps = new List<float>();
        for (int i = 0; i < lightCount; i++)
        {
            steps.Add((_allLights[i].light.intensity - (_allLights[i].intensity * targetRatio)) / (60f * dimTime));
        }

        while (time < dimTime)
        {
            for (int i = 0; i < lightCount; i++)
            {
                _allLights[i].light.intensity -= steps[i];
            }
            time += timeStep;
            yield return new WaitForSeconds(timeStep);
        }

        foreach (var data in _allLights)
        {
            data.light.intensity = data.intensity * targetRatio;
        }

        _currentLightRoutine = null;
    }

    public struct LightData
    {
        public Light2D light;
        public float intensity;

        public LightData(Light2D light, float intensity)
        {
            this.light = light;
            this.intensity = intensity;
        }
    }
}
