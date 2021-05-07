using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovableLogbook : MovableObject
{
    public static Action<MovableLogbook> logbookInitialized;
    public static Action<bool> pageChange;
    public static Action logbookPickup;
    public static Action logbookDrop;

    public ObjectInteractionZone leftCorner;
    public ObjectInteractionZone rightCorner;

    public List<LogbookPage> logbookPages;
    private LogbookPage _currentPage;
    private int _currentPageIndex;
    private int _pageCount;

    private Canvas _canvas;
    public Canvas MainCanvas { get { return _canvas; } }

    protected override void Awake()
    {
        base.Awake();

        _canvas = GetComponentInChildren<Canvas>();

        leftCorner.interactRequest += PreviousPage;
        rightCorner.interactRequest += NextPage;
    }

    protected override void Start()
    {
        base.Start();

        foreach (var page in logbookPages)
        {
            page.InitializePage(this);
        }
        _pageCount = logbookPages.Count;

        DisplayStartingPage();

        if (logbookInitialized != null)
            logbookInitialized(this);
    }

    public override void ToggleRenderers(bool toggleON)
    {
        if (!toggleON)
            base.ToggleRenderers(toggleON);
        else
        {
            _spriteRenderer.enabled = true;
            DisplayStartingPage();
        }

    }

    public override void Select(bool fireEvent = true)
    {
        base.Select(fireEvent);

        if (logbookPickup != null)
            logbookPickup();
    }

    public override void Deselect(bool fireEvent = true)
    {
        base.Deselect(fireEvent);
        if (logbookDrop != null)
            logbookDrop();
    }

    private void NextPage(ObjectInteractionZone interactionZone)
    {
        ChangePage(_currentPageIndex + 1);
        if (pageChange != null)
            pageChange(false);

    }

    private void PreviousPage(ObjectInteractionZone interactionZone)
    {
        ChangePage(_currentPageIndex - 1);
        if (pageChange != null)
            pageChange(true);
    }

    public void AddPage(int index, LogbookPage newPage)
    {
        logbookPages.Insert(index, newPage);
        newPage.InitializePage(this);
        newPage.HidePage();
        _pageCount++;

        AssignCorners();
    }

    public int IndexOf(LogbookPage targetPage)
    {
        return logbookPages.IndexOf(targetPage);
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
    }

    private void AssignCorners()
    {
        // IF first page
        if (_currentPageIndex == 0)
        {
            leftCorner.ToggleZone(false);
        }

        // IF last page
        if (_currentPageIndex == _pageCount - 1)
        {
            rightCorner.ToggleZone(false);
        }

        // IF has previous page
        if (_currentPageIndex - 1 >= 0)
        {
            leftCorner.ToggleZone(true);
        }
            
        // IF has next page
        if (_currentPageIndex == 0)
        {
            rightCorner.ToggleZoneWithoutSprite(true);
        }
        else if (_currentPageIndex + 1 <= _pageCount - 1)
        {
            rightCorner.ToggleZone(true);
        }
            
    }

    public override void ToggleInteractions(bool toggleON)
    {
        _collider.enabled = toggleON;
    }
}
