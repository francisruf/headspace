using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Settings")]
    public float sfxFadeSpeed;
    public float musicFadeSpeed;

    [Header("Sound DB")]
    public Sound[] sounds;
    [HideInInspector] public Sound currentMusic;

    private List<LoopingSoundInfo> _currentLoopingSounds = new List<LoopingSoundInfo>();

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source0 = gameObject.AddComponent<AudioSource>();
            s.source0.clip = s.clip;
            s.source0.volume = s.volume;
            s.source0.pitch = s.pitch;
            s.source0.panStereo = s.stereoPan;
            //s.source.loop = s.loop;

            if (s.loop)
            {
                s.source1 = gameObject.AddComponent<AudioSource>();
                s.source1.clip = s.clip;
                s.source1.volume = s.volume;
                s.source1.pitch = s.pitch;
                s.source1.panStereo = s.stereoPan;
                //s.source2.loop = s.loop;
            }
        }
        SceneManager.activeSceneChanged += AssignMusicOnScene;
    }

    private void OnEnable()
    {
        ShredderSlot.shredderStarted += ShredderStarted;
        ShredderSlot.shredderStopped += ShredderStopped;
        ButtonController.buttonPress += ButtonPress;
        KeyPadButtonController.buttonPress += ButtonPress;
        DropZone_Outbox.commandSuccess += CommandGood;
        DropZone_Outbox.commandFail += DrawerError;
        MovableObject.movableObjectSelected += OnMovableObjectSelected;
        MovableObject.movableObjectDeselected += OnMovableObjectDeselected;
        MovableObject.movableObjectMoving += OnMovableObjectMoving;
        MovableMarker.markerPinnedOnTile += MarkerPin;

        MainMenuController.playButtonPressed += MenuButtonSelect;
        MainMenuController.quitButtonPressed += MenuButtonSelect;
        EndMenuController.playAgainButtonPressed += MenuButtonSelect;
        EndMenuController.quitButtonPressed += MenuButtonSelect;

        KeyPadController.keypadOpen += NumPadOut;
        KeyPadController.keypadClose += NumPadClose;
        RouteScreenController.routeScreenOpen += NumPadOut;
        RouteScreenController.routeScreenClose += NumPadClose;

        SlidableTool.toolOpened += ToolOpen;
        SlidableTool.toolClosed += ToolClose;
        SlidableOutbox.outboxAutoOpen += DrawerAutoOpen;

        MapPointOfInterest.newDiscovery += NewDiscovery;
        Ship.routeFinished += ShipRouteFinished;
        Ship.shipDisabled += ShipDisabled;
        MessageManager.newMessageReceived += MessageReceived;
        MovableMessage.messageTearedFromReceiver += MessageRip;
        ContractManager.newContractReceived += NewContract;
        MovableLogbook.pageChange += PageTurn;
        MovableLogbook.logbookPickup += ManualPickup;
        MovableLogbook.logbookDrop += ManualDrop;
        Receiver.singlePrint += SinglePrint;
        MovableCommand.commandSinglePrint += SinglePrint;
        WritingMachineController.lightFlash += VoyantFlash;
        WritingMachineKeyboard.keyPress += KeyPress;
        MovableCommand.commandInOutbox += CommandInOutbox;

        SlidableTool.toolOpening += SlidingOpening;
        SlidableTool.toolClosing += SlidingClosing;

        MovableContract.contractAssigned += ContractSpark;

        TimeManager.levelTimerEndPreTrigger += LevelOver;
    }

    private void OnDisable()
    {
        ShredderSlot.shredderStarted -= ShredderStarted;
        ShredderSlot.shredderStopped -= ShredderStopped;
        ButtonController.buttonPress -= ButtonPress;
        KeyPadButtonController.buttonPress -= ButtonPress;
        DropZone_Outbox.commandSuccess -= CommandGood;
        DropZone_Outbox.commandFail -= DrawerError;
        MovableObject.movableObjectSelected -= OnMovableObjectSelected;
        MovableObject.movableObjectDeselected -= OnMovableObjectDeselected;
        MovableObject.movableObjectMoving -= OnMovableObjectMoving;
        MovableMarker.markerPinnedOnTile -= MarkerPin;

        MainMenuController.playButtonPressed -= MenuButtonSelect;
        MainMenuController.quitButtonPressed -= MenuButtonSelect;
        EndMenuController.playAgainButtonPressed -= MenuButtonSelect;
        EndMenuController.quitButtonPressed -= MenuButtonSelect;

        KeyPadController.keypadOpen -= NumPadOut;
        KeyPadController.keypadClose -= NumPadClose;
        RouteScreenController.routeScreenOpen -= NumPadOut;
        RouteScreenController.routeScreenClose -= NumPadClose;

        SlidableTool.toolOpened -= ToolOpen;
        SlidableTool.toolClosed -= ToolClose;
        SlidableOutbox.outboxAutoOpen -= DrawerAutoOpen;

        MapPointOfInterest.newDiscovery -= NewDiscovery;
        Ship.routeFinished -= ShipRouteFinished;
        Ship.shipDisabled -= ShipDisabled;
        MessageManager.newMessageReceived -= MessageReceived;
        MovableMessage.messageTearedFromReceiver -= MessageRip;
        ContractManager.newContractReceived -= NewContract;
        MovableLogbook.pageChange -= PageTurn;
        MovableLogbook.logbookPickup -= ManualPickup;
        MovableLogbook.logbookDrop -= ManualDrop;
        Receiver.singlePrint -= SinglePrint;
        MovableCommand.commandSinglePrint -= SinglePrint;
        WritingMachineController.lightFlash -= VoyantFlash;
        WritingMachineKeyboard.keyPress -= KeyPress;
        MovableCommand.commandInOutbox -= CommandInOutbox;

        SlidableTool.toolOpening -= SlidingOpening;
        SlidableTool.toolClosing -= SlidingClosing;

        MovableContract.contractAssigned -= ContractSpark;

        TimeManager.levelTimerEndPreTrigger -= LevelOver;
    }

    //Update function only to test feature. Remove when necessary.
    private void Update()
   {
       // if (Input.GetKeyDown(KeyCode.S))
        //{
        //   PlaySound("Contract_Spark_One");
       // }

        //    if (Input.GetKeyDown(KeyCode.D))
        //    {
        //        StopSoundLoop("Shredder_Loop", true);
        //    }
    }

    #region Main functions
    private void AssignMusicOnScene(Scene scene1, Scene scene2)
    {

        //if (SceneManager.GetActiveScene().name == "MainMenu") {
        //    PlayTheme("MainMenu_Loop");
        //}

        //if (SceneManager.GetActiveScene().name == "MainMenu") {
        //    if (currentMusic.source != null) {
        //        currentMusic.source.Stop();
        //    }
        //}
    }

    public void PlaySound(string name, bool fadeIn = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source0.volume = s.volume;
        if (s.source1 != null)
            s.source1.volume = s.volume;

        if (fadeIn)
            StartCoroutine(FadeIn(s.volume, s.source0));

        else
            s.source0.Play();
    }

    public void PlayRandomSound(params string[] soundNames)
    {
        
        int count = soundNames.Length;
        int randomIndex = UnityEngine.Random.Range(0, count);
        PlaySound(soundNames[randomIndex]);
    }

    public void PlayTheme(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (currentMusic.source0 != null)
            currentMusic.source0.Stop();

        currentMusic = s;
        currentMusic.source0.volume = s.volume;
        currentMusic.source0.Play();
    }

    private Sound FindSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    public void PlaySoundLoop(string name, bool dynamicSound, bool fadeIn = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }


        for (int i = 0; i < _currentLoopingSounds.Count; i++)
        {
            if (_currentLoopingSounds[i].sound == s)
                return;
        }

        LoopingSoundInfo newInfo = new LoopingSoundInfo(s, LoopSound(s, dynamicSound, true));
        _currentLoopingSounds.Add(newInfo);
        StartCoroutine(newInfo.loopingRoutine);
    }

    public void PlaySoundLoop(string name, out Sound s, bool dynamicSound, bool fadeIn = false)
    {
        s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        for (int i = 0; i < _currentLoopingSounds.Count; i++)
        {
            if (_currentLoopingSounds[i].sound == s)
                return;
        }

        LoopingSoundInfo newInfo = new LoopingSoundInfo(s, LoopSound(s, dynamicSound, true));
        _currentLoopingSounds.Add(newInfo);
        StartCoroutine(newInfo.loopingRoutine);
    }

    private IEnumerator LoopSound(Sound s, bool dynamicSound, bool fadeIn = false)
    {
        s.activeSource = 0;
        float clipLength = s.source0.clip.length;
        float fadeSpeed = 0.25f;

        s.source0.volume = s.volume;
        if (s.source1 != null)
            s.source1.volume = s.volume;

        float sourceVolume = s.volume;
        AudioSource source0 = s.source0;
        AudioSource source1 = s.source1;
        int activeSource = 0;

        if (fadeIn)
            StartCoroutine(FadeIn(sourceVolume, source0));
        else
            source0.Play();

        while (true)
        {
            if (activeSource == 0)
            {
                if (clipLength - source0.time < fadeSpeed)
                {
                    activeSource = 1;
                    StartCoroutine(FadeIn(sourceVolume, source1, fadeSpeed));
                    yield return new WaitForSeconds(fadeSpeed * 0.5f);
                    StartCoroutine(FadeOut(source0, fadeSpeed));
                }
            }
            else if (activeSource == 1)
            {
                if (clipLength - source1.time < fadeSpeed)
                {
                    activeSource = 0;
                    StartCoroutine(FadeIn(sourceVolume, source0, fadeSpeed));
                    yield return new WaitForSeconds(fadeSpeed * 0.5f);
                    StartCoroutine(FadeOut(source1, fadeSpeed));
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void StopSoundLoop(string name, bool fadeOut = false)
    {
        LoopingSoundInfo targetInfo = default;

        for (int i = 0; i < _currentLoopingSounds.Count; i++)
        {
            if (_currentLoopingSounds[i].sound.name == name)
                targetInfo = _currentLoopingSounds[i];
        }

        if (targetInfo.Equals(default(LoopingSoundInfo)))
            return;

        AudioSource targetSource = null;
        if (targetInfo.sound.activeSource == 0)
            targetSource = targetInfo.sound.source0;
        else if (targetInfo.sound.activeSource == 1)
            targetSource = targetInfo.sound.source1;

        if (fadeOut)
            StartCoroutine(FadeOut(targetSource));
        else
            targetSource.Stop();

        StopCoroutine(targetInfo.loopingRoutine);
        _currentLoopingSounds.Remove(targetInfo);
    }

    private IEnumerator FadeIn(float targetVolume, AudioSource source, float fadeSpeed = 0f)
    {
        if (fadeSpeed < 0.01f)
            fadeSpeed = sfxFadeSpeed;

        source.volume = 0f;
        float time = 0f;

        source.Play();

        while (time < fadeSpeed)
        {
            if (Mathf.Approximately(targetVolume, source.volume))
                break;

            source.volume = Mathf.Clamp(source.volume + (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator FadeOut(AudioSource source, float fadeSpeed = 0f)
    {
        if (fadeSpeed < 0.01f)
            fadeSpeed = sfxFadeSpeed;

        float startVolume = source.volume;
        float time = 0f;

        while (time < fadeSpeed)
        {
            if (Mathf.Approximately(0f, source.volume))
                break;

            source.volume = Mathf.Clamp(source.volume - (Time.deltaTime / fadeSpeed * startVolume), 0f, startVolume);
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion
    #region Tools
    private void ShredderStarted()
    {
        PlaySoundLoop("Shredder_Loop", false);
    }

    private void ShredderStopped()
    {
        StopSoundLoop("Shredder_Loop", true);
    }

    #endregion
    #region drawers

    private void ToolOpen(SlidableToolType toolType)
    {
        switch (toolType)
        {
            case SlidableToolType.Drawer:
                PlaySound("Drawer_Open_Two");
                break;
            case SlidableToolType.Outbox:
                PlaySound("Drawer_Open_Two");
                break;
            case SlidableToolType.WritingMachine:
                break;
            case SlidableToolType.Board:
                PlaySound("Drawer_Open_Two");
                break;
            default:
                break;
        }
    }

    private void ToolClose(SlidableToolType toolType)
    {
        switch (toolType)
        {
            case SlidableToolType.Drawer:
                PlaySound("Drawer_Close_One");
                break;
            case SlidableToolType.Outbox:
                PlaySound("Drawer_Close_One");
                break;
            case SlidableToolType.WritingMachine:
                break;
            case SlidableToolType.Board:
                PlaySound("Board_Close");
                break;
            default:
                break;
        }
    }

    private void SlidingOpening(SlidableToolType toolType)
    {
        switch (toolType)
        {
            case SlidableToolType.Drawer:
                PlaySound("Drawer_Pull_One");
                break;
            case SlidableToolType.Outbox:
                PlaySound("Drawer_Pull_One");
                break;
            case SlidableToolType.WritingMachine:
                PlaySound("Outil_Open");
                break;
            case SlidableToolType.Board:
                PlaySound("Board_Drag_Open");
                break;
            case SlidableToolType.Shredder:
                PlaySound("Shredder_Pull_Out");
                break;
            default:
                break;
        }
    }

    private void SlidingClosing(SlidableToolType toolType)
    {
        switch (toolType)
        {
            case SlidableToolType.Drawer:
                PlaySound("Drawer_Pull_Two");
                break;
            case SlidableToolType.Outbox:
                PlaySound("Drawer_Pull_Two");
                break;
            case SlidableToolType.WritingMachine:
                PlaySound("Outil_Open");
                break;
            case SlidableToolType.Board:
                PlaySound("Board_Drag_Close");
                break;
            case SlidableToolType.Shredder:
                break;
            default:
                break;
        }
    }

    private void DrawerAutoOpen()
    {
        PlaySound("Drawer_Open_One");
    }



    private void DrawerDrag()
    {
        PlayRandomSound("Drawer_Drag_One", "Drawer_Drag_Two", "Drawer_Drag_Three");
    }

    private void DrawerPull()
    {
        PlayRandomSound("Drawer_Pull_One", "Drawer_Pull_Two");
    }

    #endregion
    #region commands
    private void DrawerError()
    {
        // PlaySound("Error_One");
        PlaySound("Command_Wrong_Two");
    }

    private void CommandGood()
    {
        PlaySound("Command_Good");
    }

    private void CommandInOutbox()
    {
        PlaySound("Message_Drop_inDrawer");
    }
    #endregion
    #region movableObjects
    private void OnMovableObjectSelected(MovableObject obj)
    {
        switch (obj.objectType)
        {
            case ObjectType.Marker:
                MarkerTake();
                break;
            case ObjectType.Document:
                PaperPickup();
                break;
            case ObjectType.Message:
                PaperPickup();
                break;
            case ObjectType.Other:
                break;
            case ObjectType.Contract:
                PaperPickup();
                break;
            default:
                break;
        }
    }

    private void OnMovableObjectDeselected(MovableObject obj)
    {
        switch (obj.objectType)
        {
            case ObjectType.Marker:
                MarkerDrop();
                break;
            case ObjectType.Document:
                PaperDrop();
                break;
            case ObjectType.Message:
                PaperDrop();
                break;
            case ObjectType.Other:
                break;
            case ObjectType.Contract:
                PaperDrop();
                break;
            default:
                break;
        }
    }

    private void OnMovableObjectMoving(MovableObject obj)
    {
        switch (obj.objectType)
        {
            case ObjectType.Marker:
                break;
            case ObjectType.Document:
                MessageDrop();
                break;
            case ObjectType.Message:
                MessageDrop();
                break;
            case ObjectType.Other:
                break;
            case ObjectType.Contract:
                MessageDrop();
                break;
            default:
                break;
        }
    }


    private void PaperPickup()
    {
        PlaySound("Paper_Pickup");
    }

    private void PaperDrop()
    {
        PlaySound("Paper_Drop");
    }

    private void CoinReceived()
    {
        PlaySound("Coin_Received");
    }

    private void CoinStack()
    {
        PlaySound("Coin_Stack");
    }

    private void CoinStackSingle()
    {
        PlaySound("Coin_Stack_Single");
    }

    private void MessagePick()
    {
        PlaySound("Message_Pick");
    }

    private void MessageDrop()
    {
        PlaySound("Message_Drop");
    }

    private void MarkerDrop()
    {
        PlaySound("Marker_Drop");
    }
    private void MarkerPin(MovableMarker marker)
    {
        PlaySound("Marker_Pin");
    }
    private void MarkerTake()
    {
        PlaySound("Marker_Take");
    }
    private void PageTurn(bool previousPage)
    {
        if (previousPage)
            PlaySound("PageTurn_One");
        else
            PlaySound("PageTurn_Two");
    }
    private void ManualPickup()
    {
        PlaySound("Manual_Drop");
    }
    private void ManualDrop()
    {
        PlaySound("Manual_PickUp");
    }


    #endregion
    #region menus
    private void MenuButtonSelect()
    {
        PlaySound("Menu_Button_Select");
    }
    #endregion
    #region writing machine
    private void NumPadOut()
    {
        PlaySound("Numpad_Out");
    }

    private void NumPadClose()
    {
        PlaySound("Numpad_Close");
    }

    private void ButtonPress()
    {
        PlayRandomSound("Button_One", "Button_Two");
    }

    private void VoyantFlash(bool isOn)
    {
        if (isOn)
            PlaySound("Writer_Voyant_Two");
        else
            PlaySound("Writer_Voyant_One");
    }

    private void KeyPress(char c)
    {
        PlayRandomSound("Writer_Keyboard_Click_One", "Writer_Keyboard_Click_Two");
    }

    private void WriterSignal()
    {
        PlaySound("Writer_Signal");
    }

    private void WriterReady()
    {
        PlaySoundLoop("Writer_Voyant", false);
    }

    private void WriterNotReady()
    {
        StopSoundLoop("Writer_Voyant", false);
    }
    #endregion
    #region Ships
    private void NewDiscovery()
    {
        PlaySound("Ship_Route_Discovery");
    }
    private void ShipRouteFinished(Ship ship)
    {
        PlaySound("Ship_Route_End");
    }
    private void ShipDisabled(Ship ship)
    {
        PlaySound("Alert_Anomalie");
    }
    #endregion
    #region Messages
    private void MessageReceived(bool playSound)
    {
        if (playSound)
            PlaySound("Message_Received");
    }
    private void MessageRip()
    {
        PlayRandomSound("Message_Rip_One", "Message_Rip_Two");
    }

    private void SinglePrint()
    {
        PlaySound("Paper_Print_Solo");
    }

    private void SinglePrint(bool lastPrint)
    {
        if (lastPrint)
            PlaySound("Paper_Print_One");
        else
            PlaySound("Paper_Print_Solo");
    }
    

    #endregion
    #region Contracts
    private void NewContract()
    {
        PlaySound("Contract_Print");
    }

    private void ContractSpark(MovableContract contract)
    {
        PlayRandomSound("Contract_Spark_One", "Contract_Spark_Two");
    }
    #endregion
    #region Game loop
    private void LevelOver()
    {
        PlaySound("Timer_DayEnd");
    }
    #endregion


}

public struct LoopingSoundInfo
{
    public Sound sound;
    public IEnumerator loopingRoutine;

    public LoopingSoundInfo(Sound sound, IEnumerator loopingRoutine)
    {
        this.sound = sound;
        this.loopingRoutine = loopingRoutine;
    }
}
