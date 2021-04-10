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

    private bool _isOpen;
    private IEnumerator _currentAnimRoutine;

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
            if (lenght < 3)
            {
                _allTextValues[_currentScreenIndex] += c;
                if (lenght + 1 >= 3)
                    _currentScreenIndex = Mathf.Clamp(_currentScreenIndex + 1, 0, allScreens.Count - 1);

                UpdateTexts();
            }
            else
            {
                int previousScreen = _currentScreenIndex;
                _currentScreenIndex = Mathf.Clamp(_currentScreenIndex + 1, 0, allScreens.Count - 1);
                if (previousScreen != _currentScreenIndex)
                {
                    _allTextValues[_currentScreenIndex] += c;
                    UpdateTexts();
                }

            }
        }
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
        for (int i = 0; i < allScreens.Count; i++)
        {
            if (i == _currentScreenIndex)
            {
                allScreens[i].text = "<color=#EEDF84>" + _allTextValues[i] + "</color>";
            }
            else
            {
                allScreens[i].text = _allTextValues[i];
            }
        }
        float rectWidth = allScreens[_currentScreenIndex].GetComponent<RectTransform>().rect.width;
        int textLength = _allTextValues[_currentScreenIndex].Length;
        Vector2 caretPos = allScreens[_currentScreenIndex].transform.position;
        caretPos.x -= rectWidth / 2f;
        caretPos.x += textLength * (rectWidth / 3f);
        caret.transform.position = caretPos;

        if (_currentScreenIndex == allScreens.Count - 1 && textLength >= 3)
            caretAnimator.SetBool("Complete", true);
        else
            caretAnimator.SetBool("Complete", false);
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

        _currentScreenIndex = 0;
        UpdateTexts();
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
        }
    }


    private IEnumerator RevealRouteScreen()
    {
        animator.SetBool("IsOpen", true);
        yield return new WaitForSeconds(0.15f);

        if (routeScreenOpen != null)
            routeScreenOpen();

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
