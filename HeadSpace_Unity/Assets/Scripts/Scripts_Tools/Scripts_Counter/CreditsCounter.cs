using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreditsCounter : MonoBehaviour
{
    public static Action newCredits;
    public static Action pointsCalculationEnd;

    private Animator _creditsLightAnimator;
    private TextMeshProUGUI _creditsText;
    private int _currentCreditsDisplayed;
    private int _currentCreditsValue;

    private int _currentDifference;
    private IEnumerator _currentRoutine;

    public Sprite[] digits;
    public Sprite emptySprite;
    public SpriteRenderer[] digitRenderers;

    

    private void Awake()
    {
        _creditsLightAnimator = GetComponent<Animator>();
        _creditsText = GetComponentInChildren<TextMeshProUGUI>();
        UpdateText();
    }

    private void OnEnable()
    {
        //RessourceManager.creditsUpdate += OnRessourceUpdate;
        DropZone_CompletedContracts.newPointsInDropZone += OnNewPointsInDropZone;
    }

    private void OnDisable()
    {
        //RessourceManager.creditsUpdate -= OnRessourceUpdate;
        DropZone_CompletedContracts.newPointsInDropZone -= OnNewPointsInDropZone;
    }

    private void OnNewPointsInDropZone(int newCredits)
    {
        _currentCreditsValue += newCredits;
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

        yield return new WaitForSeconds(0.75f);

        while (_currentCreditsDisplayed != _currentCreditsValue)
        {
            _creditsLightAnimator.SetBool("NewCredits", true);
            _currentCreditsDisplayed += 1 * multiplier;
            UpdateText();

            if (newCredits != null)
                newCredits();

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
        }
        _creditsLightAnimator.SetBool("NewCredits", false);
        if (pointsCalculationEnd != null)
            pointsCalculationEnd();

        _currentRoutine = null;
    }

    private void UpdateText()
    {
        int c = _currentCreditsDisplayed;

        int thousand = c / 1000;
        digitRenderers[0].sprite = digits[thousand];
        c = c % 1000;

        int hundred = c / 100;
        digitRenderers[1].sprite = digits[hundred];
        c = c % 100;

        int ten = c / 10;
        digitRenderers[2].sprite = digits[ten];
        c = c % 10;

        digitRenderers[3].sprite = digits[c];
    }
}
