using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditsCounter : MonoBehaviour
{
    private TextMeshProUGUI _creditsText;
    private int _currentCreditsDisplayed;
    private int _currentCreditsValue;

    private int _currentDifference;
    private IEnumerator _currentRoutine;

    private void Awake()
    {
        _creditsText = GetComponentInChildren<TextMeshProUGUI>();
        UpdateText();
    }

    private void OnEnable()
    {
        RessourceManager.creditsUpdate += OnRessourceUpdate;
    }

    private void OnDisable()
    {
        RessourceManager.creditsUpdate -= OnRessourceUpdate;
    }

    private void OnRessourceUpdate(int currentCredits)
    {
        _currentCreditsValue = currentCredits;
        int difference = _currentCreditsValue - _currentCreditsDisplayed;

        if (_currentRoutine != null)
        {
            StopCoroutine(_currentRoutine);
            _currentRoutine = null;
            _currentCreditsDisplayed = _currentCreditsValue;
            UpdateText();
        }
        _currentRoutine = UpdateCounter(difference);
        StartCoroutine(_currentRoutine);
    }

    private IEnumerator UpdateCounter(int difference)
    {
        int multiplier = difference < 0 ? -1 : 1;

        yield return new WaitForSeconds(0.5f);

        while (_currentCreditsDisplayed != _currentCreditsValue)
        {
            _currentCreditsDisplayed += 1 * multiplier;
            UpdateText();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        _currentRoutine = null;
    }

    private void UpdateText()
    {
        _creditsText.text = _currentCreditsDisplayed.ToString("000");
    }
}
