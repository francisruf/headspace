using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogbookPage_Shipyard : LogbookPage
{
    public static Action newShipyardPage;

    [Header("Fleet page settings")]
    public int maxShipBuyableContainers;
    public GameObject pageParent;
    public GameObject shipBuyableContainerPrefab;
    public GameObject shipyardPageControllerPrefab;
    public GameObject shipyardPagePrefab;
    public VerticalLayoutGroup shipBuyablesLayoutGroup;

    private TextMeshProUGUI _creditsText;
    private TextMeshProUGUI _shipPageCountText;

    private List<Logbook_BuyableContainer> _shipBuyableContainers = new List<Logbook_BuyableContainer>();
    private int _containerCount;
    private bool _currentShipyardPage;
    private int currentShipyardPageIndex;
    public static int shipyardPageCount = 1;
    public static bool shipyardStarted;

    public override void InitializePage(MovableLogbook logbook)
    {
        base.InitializePage(logbook);

        _currentShipyardPage = true;
        currentShipyardPageIndex = shipyardPageCount;
        pageParent = Instantiate(shipyardPagePrefab, Logbook.MainCanvas.transform);
        shipBuyablesLayoutGroup = pageParent.GetComponentInChildren<VerticalLayoutGroup>();

        foreach (var txt in pageParent.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (txt.gameObject.tag == "FleetPageText")
            {
                _shipPageCountText = txt;
            }

            if (txt.gameObject.tag == "CreditsText")
            {
                _creditsText = txt;
                if (RessourceManager.instance != null)
                    OnCreditsUpdate(RessourceManager.instance.CurrentCredits);
            }
        }

        if (BuyablesDatabase.instance != null && !shipyardStarted)
        {
            OnNewShipyardDB(BuyablesDatabase.instance.ShipyardDatabase);
            shipyardStarted = true;
        }

        if (newShipyardPage != null)
            newShipyardPage();
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
        BuyablesDatabase.newShipyardDB += OnNewShipyardDB;
        LogbookPage_Shipyard.newShipyardPage += UpdateShipyardPageText;
        RessourceManager.creditsUpdate += OnCreditsUpdate;
    }

    private void OnDisable()
    {
        BuyablesDatabase.newShipyardDB -= OnNewShipyardDB;
        LogbookPage_Shipyard.newShipyardPage -= UpdateShipyardPageText;
        RessourceManager.creditsUpdate -= OnCreditsUpdate;
    }

    private void UpdateShipyardPageText()
    {
        if (_shipPageCountText != null)
        {
            _shipPageCountText.text = currentShipyardPageIndex + " / " + shipyardPageCount;
        }
    }

    private void OnNewShipyardDB(Dictionary<int, BuyableShip> catalogueDictionnary)
    {
        //Debug.Log("Received new dictionnary with : " + catalogueDictionnary.Count + " entries.");

        List<BuyableShip> allBuyables = new List<BuyableShip>();

        foreach (var item in catalogueDictionnary)
        {
            allBuyables.Add(item.Value);
        }

        StartCoroutine(PopulatePage(allBuyables));
    }

    public IEnumerator PopulatePage(List<BuyableShip> remainingObjects)
    {
        List<BuyableShip> objectsToRemove = new List<BuyableShip>();
        yield return new WaitForEndOfFrame();

        foreach (var item in remainingObjects)
        {
            if (_containerCount < maxShipBuyableContainers)
            {
                Logbook_BuyableShipContainer newContainer = Instantiate(shipBuyableContainerPrefab, shipBuyablesLayoutGroup.transform).GetComponent<Logbook_BuyableShipContainer>();
                newContainer.InitializeContainer(item);
                _shipBuyableContainers.Add(newContainer);
                _containerCount++;
                objectsToRemove.Add(item);
            }
            else
            {
                foreach (var obj in objectsToRemove)
                {
                    remainingObjects.Remove(obj);
                }

                _currentShipyardPage = false;
                shipyardPageCount++;
                LogbookPage_Shipyard newPage = Instantiate(shipyardPageControllerPrefab, transform.parent).GetComponent<LogbookPage_Shipyard>();
                newPage.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
                newPage.InitializePage(Logbook);
                int currentPageIndex = Logbook.IndexOf(this);
                Logbook.AddPage(currentPageIndex + 1, newPage);
                newPage.StartCoroutine(newPage.PopulatePage(remainingObjects));
                break;
            }
        }
    }

    private void OnCreditsUpdate(int currentCredits)
    {
        if (_creditsText != null)
            _creditsText.text = "Credits : " + currentCredits + "$";
    }
}