using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RouteScreenController : MonoBehaviour
{
    public static Action routeScreenOpen;
    public static Action routeScreenClose;
    public Animator animator;
    public Animator caretAnimator;
    public GameObject caret;

    public List<TextMeshProUGUI> allScreens = new List<TextMeshProUGUI>();
    private List<string> _allTextValues = new List<string>();
    private int _currentScreenIndex;

    public GameObject coordTextPrefab;
    private WritingMachineSpriteDB _spriteDB;
    private List<SpriteRenderer> _allCoordsRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> _templateRenders = new List<SpriteRenderer>();
    public List<Transform> _allTextPositions;

    private bool _isOpen;
    private bool _hasValues;
    private bool _templatesVisible;
    private IEnumerator _currentAnimRoutine;
    private IEnumerator _currentTemplateRoutine;

    private void Awake()
    {
        _spriteDB = GetComponentInParent<WritingMachineSpriteDB>();

        foreach (var pos in _allTextPositions)
        {
            Vector2 spawnPos = pos.position;

            for (int i = 0; i < 3; i++)
            {
                SpriteRenderer sr = Instantiate(coordTextPrefab, spawnPos, Quaternion.identity, pos).GetComponent<SpriteRenderer>();
                SpriteRenderer sr2 = Instantiate(coordTextPrefab, spawnPos, Quaternion.identity, pos).GetComponent<SpriteRenderer>();
                _allCoordsRenderers.Add(sr);
                _templateRenders.Add(sr2);
                spawnPos.x += 0.21875f;
            }
        }
    }

    private void Start()
    {
        foreach (var txt in allScreens)
        {
            txt.text = "";
            _allTextValues.Add("");
        }
        for (int i = 0; i < _allTextValues.Count; i++)
        {
            _allTextValues[i] = "";
        }
        UpdateTexts();
        InitializeTemplates();
    }

    public void OnCharInput(char c)
    {
        bool valid = false;

        c = char.ToUpper(c);
        if (c >= 'A' && c <= 'Z')
            valid = true;
        else if (c >= '0' && c <= '9')
            valid = true;

        if (valid)
        {
            int lenght = _allTextValues[_currentScreenIndex].Length;
            if (lenght == 3)
            {
                _allTextValues[_currentScreenIndex] = _allTextValues[_currentScreenIndex].Substring(0, 2) + c;
                UpdateTexts();
            }
            else if (lenght < 3)
            {
                _allTextValues[_currentScreenIndex] += c;
                //if (lenght + 1 >= 3)
                //    _currentScreenIndex = Mathf.Clamp(_currentScreenIndex + 1, 0, allScreens.Count - 1);

                UpdateTexts();
            }
            //else
            //{
            //    int previousScreen = _currentScreenIndex;
            //    _currentScreenIndex = Mathf.Clamp(_currentScreenIndex + 1, 0, allScreens.Count - 1);
            //    if (previousScreen != _currentScreenIndex)
            //    {
            //        _allTextValues[_currentScreenIndex] += c;
            //        UpdateTexts();
            //    }

            //}
        }
    }

    public void OnUpArrowInput()
    {
        int previousScreen = _currentScreenIndex;
        _currentScreenIndex = _currentScreenIndex - 1;

        if (_currentScreenIndex < 0)
            _currentScreenIndex = allScreens.Count - 1;

        UpdateTexts();
    }

    public void OnDownArrowInput()
    {
        OnTabInput();
    }

    public void OnBackspaceInput()
    {
        int lenght = _allTextValues[_currentScreenIndex].Length;
        if (lenght > 0)
        {
            _allTextValues[_currentScreenIndex] = _allTextValues[_currentScreenIndex].Substring(0, lenght - 1);
            UpdateTexts();
        }

        else if (lenght - 1 <= 0)
        {
            int previousScreen = _currentScreenIndex;
            _currentScreenIndex = Mathf.Clamp(_currentScreenIndex - 1, 0, allScreens.Count - 1);
            if (previousScreen != _currentScreenIndex)
            {
                int newLenght = _allTextValues[_currentScreenIndex].Length;
                if (newLenght > 0)
                {
                    _allTextValues[_currentScreenIndex] = _allTextValues[_currentScreenIndex].Substring(0, newLenght - 1);
                }
                UpdateTexts();
            }
        }
    }

    public void OnTabInput()
    {
        _currentScreenIndex = (_currentScreenIndex + 1) % allScreens.Count;
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        int textCount = 0;

        for (int i = 0; i < _allTextValues.Count; i++)
        {
            int count = 0;
            for (int j = 0; j < _allTextValues[i].Length; j++)
            {
                _allCoordsRenderers[(i * 3) + j].sprite = _spriteDB.GetCoordChar(_allTextValues[i][j]);
                count++;
                textCount++;
            }
            if (count < 3)
            {
                for (int h = count; h < 3; h++)
                {
                    _allCoordsRenderers[i * 3 + h].sprite = null;
                }
            }
        }

        int textLength = _allTextValues[_currentScreenIndex].Length;
        if (textLength == 3)
            textLength = 2;

        int index = Mathf.Clamp((_currentScreenIndex * 3) + textLength, 0, 17);
        Vector2 targetCaretPos = _allCoordsRenderers[index].transform.position;
        caret.transform.position = targetCaretPos;

        if (_currentScreenIndex == allScreens.Count - 1 && textLength >= 3)
            caretAnimator.SetBool("Complete", true);
        else
            caretAnimator.SetBool("Complete", false);

        _hasValues = textCount > 0;

        if (_currentTemplateRoutine != null)
        {
            StopCoroutine(_currentTemplateRoutine);
            _currentTemplateRoutine = null;
        }
        foreach (var sr in _templateRenders)
        {
            sr.enabled = false;
        }
        _templatesVisible = false;
    }

    public void ResetTexts()
    {
        foreach (var txt in allScreens)
        {
            txt.text = "";
        }
        for (int i = 0; i < _allTextValues.Count; i++)
        {
            _allTextValues[i] = "";
        }

        _hasValues = false;
        _currentScreenIndex = 0;
        UpdateTexts();
        UpdateTemplates();
    }

    private void InitializeTemplates()
    {
        int currentNumber = 1;
        for (int i = 0; i < _templateRenders.Count; i++)
        {
            if (i % 3 == 0)
            {
                _templateRenders[i].sprite = _spriteDB.GetCoordChar('A');
            }
            else if ((i-1) % 3 == 0)
            {
                _templateRenders[i].sprite = _spriteDB.GetCoordChar('0');
            }
            else
            {
                _templateRenders[i].sprite = _spriteDB.GetCoordChar(currentNumber.ToString()[0]);
                currentNumber++;
            }
        }
        foreach (var sr in _templateRenders)
        {
            sr.enabled = false;
        }
    }

    private void UpdateTemplates()
    {
        if (!_templatesVisible && !_hasValues)
        {
            if (_currentTemplateRoutine == null)
            {
                _currentTemplateRoutine = TemplatesFlashing();
                StartCoroutine(_currentTemplateRoutine);
            }
        }

        else if (_templatesVisible && _hasValues)
        {
            if (_currentTemplateRoutine != null)
            {
                StopCoroutine(_currentTemplateRoutine);
                _currentTemplateRoutine = null;
            }
            foreach (var sr in _templateRenders)
            {
                sr.enabled = false;
            }
            _templatesVisible = false;
        }
    }

    private IEnumerator TemplatesFlashing()
    {
        _templatesVisible = true;
        int count = 0;

        yield return new WaitForSeconds(0.3f);

        while (count < 3)
        {
            foreach (var sr in _templateRenders)
            {
                sr.enabled = true;
            }
            yield return new WaitForSeconds(0.3f);

            foreach (var sr in _templateRenders)
            {
                sr.enabled = false;
            }
            yield return new WaitForSeconds(0.1f);
            count++;
        }
        foreach (var sr in _templateRenders)
        {
            sr.enabled = false;
        }
        _templatesVisible = false;
        _currentTemplateRoutine = null;
    }

    public void OnDirectionInput(KeyCode input)
    {
    }

    public void OnEnterInput()
    {
    }

    public List<string> GetRouteInput()
    {
        List<string> route = new List<string>();
        foreach (var txt in _allTextValues)
        {
            if (txt.Length > 0)
                route.Add(txt);
        }
        return route;
    }

    public void OpenRouteScreen()
    {
        if (_isOpen)
        {
            ResetTexts();
        }
        else
        {
            if (_currentAnimRoutine == null)
            {
                _currentAnimRoutine = RevealRouteScreen();
                StartCoroutine(_currentAnimRoutine);
            }
            else
            {
                StopCoroutine(_currentAnimRoutine);
                _currentAnimRoutine = RevealRouteScreen();
                StartCoroutine(_currentAnimRoutine);
            }
        }
        UpdateTemplates();
    }


    private IEnumerator RevealRouteScreen()
    {
        if (routeScreenOpen != null)
            routeScreenOpen();

        animator.SetBool("IsOpen", true);
        UpdateTemplates();
        yield return new WaitForSeconds(0.15f);
        yield return new WaitForSeconds(0.15f);

        _isOpen = true;
        
        //_currentScreenIndex = 0;
        //ResetTexts();

        _currentAnimRoutine = null;
    }

    public void CloseRouteScreen(bool resetValues)
    {
        if (!_isOpen)
        {
            return;
        }
        else
        {
            //Debug.Log("CLOSE CALLED");
            if (_currentAnimRoutine == null)
            {
                _currentAnimRoutine = HideRouteScreen(resetValues);
                StartCoroutine(_currentAnimRoutine);
            }
        }
    }

    private IEnumerator HideRouteScreen(bool resetValues)
    {
        yield return new WaitForSeconds(0.1f);

        if (routeScreenClose != null)
            routeScreenClose();

        if (resetValues)
            ResetTexts();

        _isOpen = false;
        animator.SetBool("IsOpen", false);

        yield return new WaitForSeconds(0.4f);
        _currentAnimRoutine = null;
    }
}
