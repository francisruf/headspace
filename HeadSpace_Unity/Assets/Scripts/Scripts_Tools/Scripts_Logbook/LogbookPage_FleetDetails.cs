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

    public TextMeshProUGUI fleetPageCountText;

    private List<Logbook_ShipContainer> _shipContainers = new List<Logbook_ShipContainer>();
    private int _containerCount;
    private bool _currentFleetPage;
    private int currentFleetPageIndex;
    public static int fleetPageCount = 1;

    private void Awake()
    {
        _currentFleetPage = true;
    }

    private void Start()
    {
        currentFleetPageIndex = fleetPageCount;
        pageParent = Instantiate(fleetDetailsPagePrefab, Logbook.MainCanvas.transform);
        shipLayoutGroup = pageParent.GetComponentInChildren<VerticalLayoutGroup>();

        foreach (var txt in pageParent.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (txt.gameObject.tag == "FleetPageText")
            {
                fleetPageCountText = txt;
                break;
            }
        }

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
        LogbookPage_FleetDetails.newFleetPage += UpdateFleetPageText;
    }

    private void OnDisable()
    {
        Ship.newShipAvailable -= OnNewShipAvailable;
        Ship.shipStateChange -= OnShipStateChange;
        LogbookPage_FleetDetails.newFleetPage -= UpdateFleetPageText;
    }

    private void UpdateFleetPageText()
    {
        if (fleetPageCountText != null)
        {
            fleetPageCountText.text = currentFleetPageIndex + " / " + fleetPageCount;
        }
    }

    private void OnNewShipAvailable(Ship ship)
    {
        if (!_currentFleetPage)
            return;

        if (_containerCount < maxShipContainers)
        {
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
        _containerCount++;
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
