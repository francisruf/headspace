using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogbookPage_Catalogue : LogbookPage
{
    public static Action newCataloguePage;

    [Header("Fleet page settings")]
    public int maxBuyableContainers;
    public GameObject pageParent;
    public GameObject buyableContainerPrefab;
    public GameObject cataloguePageControllerPrefab;
    public GameObject cataloguePagePrefab;
    public VerticalLayoutGroup buyablesLayoutGroup;

    public TextMeshProUGUI cataloguePageCountText;

    private List<Logbook_BuyableContainer> _buyableContainers = new List<Logbook_BuyableContainer>();
    private int _containerCount;
    private bool _currentCataloguePage;
    private int currentCataloguePageIndex;
    public static int cataloguePageCount = 1;
    public static bool catalogueStarted;

    private void Awake()
    {
        _currentCataloguePage = true;
    }

    private void Start()
    {
        currentCataloguePageIndex = cataloguePageCount;
        pageParent = Instantiate(cataloguePagePrefab, Logbook.MainCanvas.transform);
        buyablesLayoutGroup = pageParent.GetComponentInChildren<VerticalLayoutGroup>();

        foreach (var txt in pageParent.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (txt.gameObject.tag == "FleetPageText")
            {
                cataloguePageCountText = txt;
                break;
            }
        }

        if (BuyablesDatabase.instance != null && !catalogueStarted)
        {
            OnNewCatalogueDB(BuyablesDatabase.instance.CatalogueDatabase);
            catalogueStarted = true;
        }

        if (newCataloguePage != null)
            newCataloguePage();
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
        BuyablesDatabase.newCatalogueDB += OnNewCatalogueDB;
        LogbookPage_Catalogue.newCataloguePage += UpdateCataloguePageText;
    }

    private void OnDisable()
    {
        BuyablesDatabase.newCatalogueDB -= OnNewCatalogueDB;
        LogbookPage_Catalogue.newCataloguePage -= UpdateCataloguePageText;
    }

    private void UpdateCataloguePageText()
    {
        if (cataloguePageCountText != null)
        {
            cataloguePageCountText.text = currentCataloguePageIndex + " / " + cataloguePageCount;
        }
    }

    private void OnNewCatalogueDB(Dictionary<int, BuyableObject> catalogueDictionnary)
    {
        Debug.Log("Received new dictionnary with : " + catalogueDictionnary.Count + " entries.");

        List<BuyableObject> allBuyables = new List<BuyableObject>();

        foreach (var item in catalogueDictionnary)
        {
            allBuyables.Add(item.Value);
        }

        StartCoroutine(PopulatePage(allBuyables));
    }

    public IEnumerator PopulatePage(List<BuyableObject> remainingObjects)
    {
        List<BuyableObject> objectsToRemove = new List<BuyableObject>();
        yield return new WaitForEndOfFrame();

        foreach (var item in remainingObjects)
        {
            if (_containerCount < maxBuyableContainers)
            {
                Logbook_BuyableContainer newContainer = Instantiate(buyableContainerPrefab, buyablesLayoutGroup.transform).GetComponent<Logbook_BuyableContainer>();
                newContainer.InitializeContainer(item);
                _buyableContainers.Add(newContainer);
                _containerCount++;
                objectsToRemove.Add(item);
            }
            else
            {
                foreach (var obj in objectsToRemove)
                {
                    remainingObjects.Remove(obj);
                }

                _currentCataloguePage = false;
                cataloguePageCount++;
                LogbookPage_Catalogue newPage = Instantiate(cataloguePageControllerPrefab, transform.parent).GetComponent<LogbookPage_Catalogue>();
                newPage.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
                int currentPageIndex = Logbook.IndexOf(this);
                Logbook.AddPage(currentPageIndex + 1, newPage);
                newPage.StartCoroutine(newPage.PopulatePage(remainingObjects));
                break;
            }
        }
    }

    //private void OnNewShipAvailable(Ship ship)
    //{
    //    if (!_currentFleetPage)
    //        return;

    //    if (_containerCount < maxShipContainers)
    //    {
    //        StartCoroutine(AddShip(ship));
    //    }
    //    else
    //    {
    //        _currentFleetPage = false;
    //        fleetPageCount++;
    //        LogbookPage_FleetDetails newPage = Instantiate(fleetDetailsPageControllerPrefab, transform.parent).GetComponent<LogbookPage_FleetDetails>();
    //        newPage.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
    //        int currentPageIndex = Logbook.IndexOf(this);
    //        Logbook.AddPage(currentPageIndex + 1, newPage);
    //        newPage.StartCoroutine(newPage.AddShip(ship));
    //    }
    //}

    //public IEnumerator AddShip(Ship ship)
    //{
    //    yield return new WaitForEndOfFrame();
    //    Logbook_ShipContainer newContainer = Instantiate(shipContainerPrefab, shipLayoutGroup.transform).GetComponent<Logbook_ShipContainer>();
    //    newContainer.InitializeContainer(ship);
    //    _shipContainers.Add(newContainer);
    //    _containerCount++;
    //}
}
