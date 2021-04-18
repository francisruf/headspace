using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Action gameStarted;
    public static Action gameEnded;
    public static Action gameOver;
    public static bool GameStarted { get; private set; }

    public float LevelDurationInMinutes { get { return levelDurationInMinutes; } }

    [Header("Level duration")]
    public float levelDurationInMinutes;

    [Header("Cursor settings")]
    public Sprite baseCursor;
    public Sprite objectCursor;
    public Sprite interactCursor;
    public GameObject cursorObj;
    public LayerMask objectLayers;
    private SpriteRenderer _cursorRenderer;

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

        _cursorRenderer = cursorObj.GetComponent<SpriteRenderer>();
        Cursor.visible = false;
    }

    private void Update()
    {
        MoveCursor();
        SetCursor();
    }

    private void OnEnable()
    {
        GameStarted = false;

        Command_Send.playerFirstDeploy += OnFirstDeploy;
        TimeManager.levelTimerEnded += OnLevelTimerEnded;
        PlanetManager.noMoreSoulsInSector += OnNoMoreSoulsInSector;
    }

    private void OnDisable()
    {
        Command_Send.playerFirstDeploy -= OnFirstDeploy;
        TimeManager.levelTimerEnded -= OnLevelTimerEnded;
        PlanetManager.noMoreSoulsInSector -= OnNoMoreSoulsInSector;
    }

    private void OnFirstDeploy()
    {
        GameStarted = true;
        
        //StartCoroutine(CheckPlayerRessources());

        if (gameStarted != null)
            gameStarted();

        Debug.Log("GAME STARTED");
    }

    public void ForceStartGame()
    {
        OnFirstDeploy();
    }

    private void OnLevelTimerEnded()
    {
        EndSector();
    }

    private void OnNoMoreSoulsInSector()
    {
        EndSector();
    }

    private void EndSector()
    {
        GameStarted = false;

        if (gameEnded != null)
            gameEnded();
    }

    private void GameOver()
    {
        GameStarted = false;

        if (gameOver != null)
            gameOver();
    }

    private void MoveCursor()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursorObj.transform.position = mousePos;
    }

    private void SetCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hitsInfo = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, objectLayers);

        bool overObject = false;
        bool interactionZone = false;
        int length = hitsInfo.Length;

        if (length > 0)
            overObject = true;

        for (int i = 0; i < length; i++)
        {
            if (hitsInfo[i].collider.GetComponent<ObjectInteractionZone>() != null)
            {
                interactionZone = true;
                break;
            }
        }

        if (_cursorRenderer != null)
        {
            if (interactionZone)
                _cursorRenderer.sprite = interactCursor;
            else if (overObject)
                _cursorRenderer.sprite = objectCursor;
            else
                _cursorRenderer.sprite = baseCursor;
        }
    }

    //private IEnumerator CheckPlayerRessources()
    //{
    //    bool hasShips = false;
    //    bool hasEnoughCredits = false;

    //    while (GameStarted)
    //    {    
    //        yield return new WaitForSeconds(1f);

    //        hasShips = true;
    //        hasEnoughCredits = true;

    //        if (RessourceManager.instance != null)
    //            if (RessourceManager.instance.CurrentCredits < _minimumCreditsNeeded)
    //                hasEnoughCredits = false;

    //        if (ShipManager.instance != null)
    //            if (ShipManager.instance.ActiveShipsCount <= 0)
    //                hasShips = false;

    //        if (ShopManager.instance != null)
    //            if (!ShopManager.instance.TransactionInProgress)
    //            {
    //                if (hasShips == false && hasEnoughCredits == false)
    //                {
    //                    GameOver();
    //                    break;
    //                }
    //            }
    //    }
    //}
}
