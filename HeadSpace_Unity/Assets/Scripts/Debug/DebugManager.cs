using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    public GameObject buttonsPanel;
    public GameObject mouseToolTip;
    public GameObject gridDebug;

    public TextMeshProUGUI debugText;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        buttonsPanel.SetActive(false);
        debugText.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            buttonsPanel.SetActive(!buttonsPanel.activeSelf);
            debugText.enabled = !buttonsPanel.activeSelf;
        }
    }

    public void ToggleGrid()
    {
        gridDebug.SetActive(!gridDebug.activeSelf);
    }

    public void ToggleMouseCoords()
    {
        mouseToolTip.SetActive(!mouseToolTip.activeSelf);
    }

    public void SpawnRandomAnomalyTile()
    {
        if (GridManager.instance != null)
            GridManager.instance.SpawnRandomAnomalyTile();
    }
}
