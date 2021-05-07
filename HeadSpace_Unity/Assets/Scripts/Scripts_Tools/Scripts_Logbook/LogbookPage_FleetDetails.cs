using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogbookPage_FleetDetails : LogbookPage
{
    public static Action newFleetPage;

    [Header("Fleet page settings")]
    public int maxShipContainers;
    public GameObject pageParent;
    public GameObject shipContainerPrefab;
    public GameObject fleetDetailsPageControllerPrefab;
    public GameObject fleetDetailsPagePrefab;
    public VerticalLayoutGroup shipLayoutGroup;

    private List<Logbook_ShipContainer> _shipContainers = new List<Logbook_ShipContainer>();
    private int _containerCount;
    private bool _currentFleetPage;
    private int currentFleetPageIndex;
    public static int fleetPageCount = 1;

    public override void InitializePage(MovableLogbook logbook)
    {
        base.InitializePage(logbook);

        _currentFleetPage = true;

        currentFleetPageIndex = fleetPageCount;
        pageParent = Instantiate(fleetDetailsPagePrefab, Logbook.MainCanvas.transform);
        shipLayoutGroup = pageParent.GetComponentInChildren<VerticalLayoutGroup>();

        if (newFleetPage != null)
            newFleetPage();
    }

    public override void DisplayPage(out Sprite newSprite)
    {
        base.DisplayPage(out newSprite);
        pageParent.SetActive(true);
    }

    public override void HidePage()
    {
        base.HidePage();
        pageParent.SetActive(false);
    }

    private void OnEnable()
    {
        Ship.newShipAvailable += OnNewShipAvailable;
        Ship.shipStateChange += OnShipStateChange;
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipStateChange -= OnShipStateChange;
    }

    private void OnNewShipAvailable(Ship ship)
    {
        if (!_currentFleetPage)
            return;

        if (_containerCount < maxShipContainers)
        {
            _containerCount++;
            StartCoroutine(AddShip(ship));
        }
        else
        {
            _currentFleetPage = false;
            fleetPageCount++;
            LogbookPage_FleetDetails newPage = Instantiate(fleetDetailsPageControllerPrefab, transform.parent).GetComponent<LogbookPage_FleetDetails>();
            newPage.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
            int currentPageIndex = Logbook.IndexOf(this);
            Logbook.AddPage(currentPageIndex + 1, newPage);
            newPage.StartCoroutine(newPage.AddShip(ship));
        }
    }

    public IEnumerator AddShip(Ship ship)
    {
        yield return new WaitForEndOfFrame();
        Logbook_ShipContainer newContainer = Instantiate(shipContainerPrefab, shipLayoutGroup.transform).GetComponent<Logbook_ShipContainer>();
        newContainer.InitializeContainer(ship);
        _shipContainers.Add(newContainer);
    }

    private void OnShipStateChange(Ship ship)
    {
        foreach (var container in _shipContainers)
        {
            if (container.LinkedShip == ship)
            {
                container.ShipUpdate();
                return;
            }
        }
    }
}
