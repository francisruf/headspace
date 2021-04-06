﻿using UnityEngine.Audio;
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
            //s.source.loop = s.loop;

            if (s.loop)
            {
                s.source1 = gameObject.AddComponent<AudioSource>();
                s.source1.clip = s.clip;
                s.source1.volume = s.volume;
                s.source1.pitch = s.pitch;
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
        MovableMarker.markerPinnedOnTile += MarkerPin;

        MainMenuController.playButtonPressed += MenuButtonSelect;
        MainMenuController.quitButtonPressed += MenuButtonSelect;
        EndMenuController.playAgainButtonPressed += MenuButtonSelect;
        EndMenuController.quitButtonPressed += MenuButtonSelect;

        KeyPadController.keypadOpen += NumPadOut;
        KeyPadController.keypadClose += NumPadClose;

        SlidableTool.drawerOpened += DrawerOpen;
        SlidableTool.drawerClosed += DrawerClose;
        SlidableOutbox.outboxAutoOpen += DrawerAutoOpen;
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
        MovableMarker.markerPinnedOnTile -= MarkerPin;

        MainMenuController.playButtonPressed -= MenuButtonSelect;
        MainMenuController.quitButtonPressed -= MenuButtonSelect;
        EndMenuController.playAgainButtonPressed -= MenuButtonSelect;
        EndMenuController.quitButtonPressed -= MenuButtonSelect;

        KeyPadController.keypadOpen -= NumPadOut;
        KeyPadController.keypadClose -= NumPadClose;

        SlidableTool.drawerOpened -= DrawerOpen;
        SlidableTool.drawerClosed -= DrawerClose;
        SlidableOutbox.outboxAutoOpen -= DrawerAutoOpen;
    }

    //Update function only to test feature. Remove when necessary.
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.S))
    //    {
    //        PlaySoundLoop("Shredder_Loop");
    //    }

    //    if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        StopSoundLoop("Shredder_Loop", true);
    //    }
    //}

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

        if (fadeIn)
            StartCoroutine(FadeIn(s, s.source0));

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
        currentMusic.source0.Play();
    }

    private Sound FindSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    public void PlaySoundLoop(string name, bool fadeIn = false)
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

        LoopingSoundInfo newInfo = new LoopingSoundInfo(s, LoopSound(s, true));
        _currentLoopingSounds.Add(newInfo);
        StartCoroutine(newInfo.loopingRoutine);
    }

    private IEnumerator LoopSound(Sound s, bool fadeIn = false)
    {
        s.activeSource = 0;
        float clipLength = s.source0.clip.length;
        float fadeSpeed = 0.25f;

        if (fadeIn)
            StartCoroutine(FadeIn(s, s.source0));
        else
            s.source0.Play();

        while (true)
        {
            if (s.activeSource == 0)
            {
                if (clipLength - s.source0.time < fadeSpeed)
                {
                    s.activeSource = 1;
                    StartCoroutine(FadeIn(s, s.source1, fadeSpeed));
                    yield return new WaitForSeconds(fadeSpeed * 0.5f);
                    StartCoroutine(FadeOut(s, s.source0, fadeSpeed));
                }
            }
            else if (s.activeSource == 1)
            {
                if (clipLength - s.source1.time < fadeSpeed)
                {
                    s.activeSource = 0;
                    StartCoroutine(FadeIn(s, s.source0, fadeSpeed));
                    yield return new WaitForSeconds(fadeSpeed * 0.5f);
                    StartCoroutine(FadeOut(s, s.source1, fadeSpeed));
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
            StartCoroutine(FadeOut(targetInfo.sound, targetSource));
        else
            targetSource.Stop();

        StopCoroutine(targetInfo.loopingRoutine);
        _currentLoopingSounds.Remove(targetInfo);
    }

    private IEnumerator FadeIn(Sound s, AudioSource source, float fadeSpeed = 0f)
    {
        if (fadeSpeed < 0.01f)
            fadeSpeed = sfxFadeSpeed;

        float targetVolume = s.volume;
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

    private IEnumerator FadeOut(Sound s, AudioSource source, float fadeSpeed = 0f)
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

    #region Tools
    private void ShredderStarted()
    {
        PlaySoundLoop("Shredder_Loop");
    }

    private void ShredderStopped()
    {
        Debug.Log("ShredderStopped");
        StopSoundLoop("Shredder_Loop", true);
    }
    
    #endregion
    #region drawers

    private void DrawerOpen()
    {
        PlaySound("Drawer_Open_Two");
    }

    private void DrawerAutoOpen()
    {
        PlaySound("Drawer_Open_One");
    }

    private void DrawerClose()
    {
        PlayRandomSound("Drawer_Close_One", "Drawer_Close_Two");
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
        PlaySound("Error_One");
    }

    private void CommandGood()
    {
        PlaySound("Command_Good");
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
                MessagePick();
                break;
            case ObjectType.Other:
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
                MessageDrop();
                break;
            case ObjectType.Other:
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
    #endregion

    private void WriterSignal()
    {
        PlaySound("Writer_Signal");
    }

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
