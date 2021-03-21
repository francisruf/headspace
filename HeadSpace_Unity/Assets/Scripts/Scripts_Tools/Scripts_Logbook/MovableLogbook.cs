﻿using System.Collections;
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

    private Canvas _canvas;
    public Canvas MainCanvas { get { return _canvas; } }

    protected override void Awake()
    {
        base.Awake();

        _canvas = GetComponentInChildren<Canvas>();

        leftCorner.interactRequest += PreviousPage;
        rightCorner.interactRequest += NextPage;

        foreach (var page in logbookPages)
        {
            page.Logbook = this;
        }
    }

    protected override void Start()
    {
        base.Start();

        _pageCount = logbookPages.Count;

        DisplayStartingPage();
    }

    private void NextPage(ObjectInteractionZone interactionZone)
    {
        ChangePage(_currentPageIndex + 1);
    }

    private void PreviousPage(ObjectInteractionZone interactionZone)
    {
        ChangePage(_currentPageIndex - 1);
    }

    public void AddPage(int index, LogbookPage newPage)
    {
        logbookPages.Insert(index, newPage);
        _pageCount++;
        newPage.Logbook = this;
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