using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableLogbook : MovableObject
{
    public ObjectInteractionZone leftCorner;
    public ObjectInteractionZone rightCorner;

    public List<LogbookPage> logbookPages;
    private LogbookPage _currentPage;
    private int _currentPageIndex;
    private int _pageCount;

    protected override void Awake()
    {
        base.Awake();

        leftCorner.interactRequest += PreviousPage;
        rightCorner.interactRequest += NextPage;

        _pageCount = logbookPages.Count;
    }

    protected override void Start()
    {
        base.Start();
        DisplayStartingPage();
    }

    private void NextPage(ObjectInteractionZone interactionZone)
    {
        Debug.Log("NEXT PAGE CALLED");
        ChangePage(_currentPageIndex + 1);
    }

    private void PreviousPage(ObjectInteractionZone interactionZone)
    {
        Debug.Log("PREVIOUS PAGE CALLED");
        ChangePage(_currentPageIndex - 1);
    }

    private void DisplayStartingPage()
    {
        for (int i = 0; i < _pageCount; i++)
        {
            logbookPages[i].HidePage();
        }

        ChangePage(0);
    }

    private void ChangePage(int newPageIndex)
    {
        if (newPageIndex >= _pageCount || newPageIndex < 0)
            return;

        if (_currentPage != null)
            _currentPage.HidePage();

        _currentPageIndex = newPageIndex;
        _currentPage = logbookPages[_currentPageIndex];

        Sprite newSprite;
        _currentPage.DisplayPage(out newSprite);
        _spriteRenderer.sprite = newSprite;

        AssignCorners();

        Debug.Log("CURRENT PAGE : " + _currentPageIndex);
    }

    private void AssignCorners()
    {
        // IF first page
        if (_currentPageIndex == 0)
            leftCorner.ToggleZone(false);

        // IF last page
        if (_currentPageIndex == _pageCount - 1)
        {
            rightCorner.ToggleZone(false);
        }

        // IF has previous page
        if (_currentPageIndex - 1 >= 0)
            leftCorner.ToggleZone(true);

        // IF has next page
        if (_currentPageIndex + 1 <= _pageCount - 1)
            rightCorner.ToggleZone(true);
    }
}
