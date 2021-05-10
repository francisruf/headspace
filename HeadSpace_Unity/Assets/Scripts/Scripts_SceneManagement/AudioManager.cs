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
    public Sound[] music;
    private Sound _currentMusic;

    private List<LoopingSoundInfo> _currentLoopingSounds = new List<LoopingSoundInfo>();
    private IEnumerator _currentLoopingSoundscapeRoutine;
    private IEnumerator _currentLoopingThemeRoutine;
    private Sound _currentLoopingSoundscape;
    private Sound _currentLoopingTheme;

    private IEnumerator _shredderRoutine;
    private bool _shredderPlaying;

    private int _coinIndex;
    private bool _oddClockTick;
    private bool _endDay;

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

        foreach (Sound m in music)
        {
            m.loop = true;

            m.source0 = gameObject.AddComponent<AudioSource>();
            m.source0.clip = m.clip;
            m.source0.volume = m.volume;
            m.source0.pitch = m.pitch;
            m.source0.panStereo = m.stereoPan;
            //s.source.loop = s.loop;

            if (m.loop)
            {
                m.source1 = gameObject.AddComponent<AudioSource>();
                m.source1.clip = m.clip;
                m.source1.volume = m.volume;
                m.source1.pitch = m.pitch;
                m.source1.panStereo = m.stereoPan;
                //s.source2.loop = s.loop;
            }
        }

        SceneManager.activeSceneChanged += AssignMusicOnScene;
    }

    private void OnEnable()
    {
        LevelManager.mainMenuLoaded += PlayMenuSoundscape;
        LeaderboardController.leaderboardLoaded += PlayGameplaySoundscape;
        GameManager.levelStarted += PlayGameplayTheme;
        GameManager.levelEnded += StopGameplayTheme;
        GameManager.levelEnded += StopGamePlaySoundscape;
        EndOfDemoController.endOfDemoLoaded += PlayGameplayTheme;
        CutsceneController.cutsceneStarted += PlaySTMTheme;
        CutsceneController.cutsceneOver += StopSTMTheme;
        CutsceneController.cutsceneLoaded += StopMenuSoundscape;

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
        Ship.shipReEnabled += ShipRouteFinished;
        MessageManager.newMessageReceived += MessageReceived;
        MovableMessage.messageTearedFromReceiver += MessageRip;
        WritingMachineController.commandTeared += MessageRip;
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
        DropZone_CardReader.cardProcessed += CommandGood;
        CardReader.lightTick += VoyantFlash;
        CreditsCounter.newCredits += NewCreditsReceived;

        MovableScissors.scissorsCut += ScissorsCut;
        GridTile_StaticAnomaly.firstAnomalySpawned += OnFirstAnomaly;
        WritingMachineClock.clockTick += OnClockTick;
        GameManager.levelStarted += OnLevelStart;

        LevelManager.unloadingDone += OnUnloading;
    }

    private void OnDisable()
    {
        LevelManager.mainMenuLoaded -= PlayMenuSoundscape;
        LeaderboardController.leaderboardLoaded -= PlayGameplaySoundscape;
        CutsceneController.cutsceneLoaded -= StopMenuSoundscape;
        CutsceneController.cutsceneStarted -= PlaySTMTheme;
        CutsceneController.cutsceneOver -= StopSTMTheme;
        GameManager.levelStarted -= PlayGameplayTheme;
        GameManager.levelEnded -= StopGameplayTheme;
        GameManager.levelEnded -= StopGamePlaySoundscape;
        EndOfDemoController.endOfDemoLoaded -= PlayGameplayTheme;

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
        Ship.shipReEnabled -= ShipRouteFinished;
        MessageManager.newMessageReceived -= MessageReceived;
        MovableMessage.messageTearedFromReceiver -= MessageRip;
        WritingMachineController.commandTeared -= MessageRip;
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
        DropZone_CardReader.cardProcessed -= CommandGood;
        CardReader.lightTick -= VoyantFlash;
        CreditsCounter.newCredits -= NewCreditsReceived;

        MovableScissors.scissorsCut -= ScissorsCut;
        GridTile_StaticAnomaly.firstAnomalySpawned -= OnFirstAnomaly;
        WritingMachineClock.clockTick -= OnClockTick;
        GameManager.levelStarted -= OnLevelStart;

        LevelManager.unloadingDone -= OnUnloading;
    }

    //Update function only to test feature. Remove when necessary.
    private void Update()
   {
       //if (Input.GetKeyDown(KeyCode.S))
       // {
        //   PlaySound("Timer_LastHour_New");
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

    private void PlayMenuSoundscape()
    {
        PlaySoundscapeLoop("HS_Menu");
    }

    private void StopMenuSoundscape()
    {
        StopSoundscapeLoop("HS_Menu");
    }

    private void PlayGameplaySoundscape()
    {
        if (GameManager.instance.CurrentDayInfo.time == LevelTime.DayStart)
            PlaySoundscapeLoop("HS_Bgd_Gameplay");
    }

    private void StopGamePlaySoundscape()
    {
        StopSoundscapeLoop("HS_Bgd_Gameplay");
    }

    private void PlayGameplayTheme()
    {
        SimpleLoop("HS_Music_Gameplay", true);
    }

    private void StopGameplayTheme()
    {
        StopSimpleLoop("HS_Music_Gameplay", 3f);
    }

    private void PlaySTMTheme()
    {
        SimpleLoop("HS_STM_Theme", false);
    }

    private void StopSTMTheme(string str, SceneLoadType sceneLoadType)
    {
        StopSimpleLoop("HS_STM_Theme", musicFadeSpeed);
    }

    private void SimpleLoop(string name, bool fadeIn)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source0.loop = true;
        s.source0.time = 0f;

        if (fadeIn)
            StartCoroutine(FadeIn(s.volume, s.source0, 1f));
        else
        {
            s.source0.volume = s.volume;
            s.source0.Play();
        }
    }

    private void StopSimpleLoop(string name, float fadeSpeed)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        StartCoroutine(FadeOut(s.source0, fadeSpeed));
    }

    public void PlayTheme(string name, bool fadeOut)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        StartCoroutine(PlayThemeAndFade(s, fadeOut));
    }

    private IEnumerator PlayThemeAndFade(Sound s, bool fadeOut)
    {
        float lenght = s.clip.length;
        s.source0.volume = s.volume;
        s.source0.Play();

        while (s.source0.time < lenght - musicFadeSpeed && s.source0.isPlaying)
        {
            yield return new WaitForSeconds(0.05f);
        }
        if (fadeOut && s.source0.isPlaying)
        {
            yield return FadeOut(s.source0, musicFadeSpeed);
        }
    }

    private void PlayThemeLoop(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (_currentLoopingTheme != null)
        {
            StopCoroutine(_currentLoopingThemeRoutine);
            _currentLoopingThemeRoutine = null;

            AudioSource source = null;
            if (_currentLoopingTheme.activeSource == 0)
                source = _currentLoopingTheme.source0;
            else
                source = _currentLoopingTheme.source1;

            if (source != null)
                StartCoroutine(FadeOut(source, musicFadeSpeed));

            _currentLoopingTheme = null;
        }
        _currentLoopingTheme = s;
        _currentLoopingThemeRoutine = ThemeLoop(s);
        StartCoroutine(_currentLoopingThemeRoutine);
    }

    private void StopThemeLoop(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (_currentLoopingTheme == s)
        {
            StopCoroutine(_currentLoopingThemeRoutine);
            _currentLoopingThemeRoutine = null;

            AudioSource source = null;
            if (_currentLoopingTheme.activeSource == 0)
                source = _currentLoopingTheme.source0;
            else
                source = _currentLoopingTheme.source1;

            if (source != null)
                StartCoroutine(FadeOut(source, musicFadeSpeed));

            _currentLoopingTheme = null;
        }
    }

    private void StopSoundscapeLoop(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (_currentLoopingSoundscape == s)
        {
            StopCoroutine(_currentLoopingSoundscapeRoutine);
            _currentLoopingSoundscapeRoutine = null;

            AudioSource source = null;
            if (_currentLoopingSoundscape.activeSource == 0)
                source = _currentLoopingSoundscape.source0;
            else
                source = _currentLoopingSoundscape.source1;

            if (source != null)
                StartCoroutine(FadeOut(source, musicFadeSpeed));

            _currentLoopingSoundscape = null;
        }
    }

    private void PlaySoundscapeLoop(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (_currentLoopingSoundscape != null)
        {
            StopCoroutine(_currentLoopingSoundscapeRoutine);
            _currentLoopingSoundscapeRoutine = null;

            AudioSource source = null;
            if (_currentLoopingSoundscape.activeSource == 0)
                source = _currentLoopingSoundscape.source0;
            else
                source = _currentLoopingSoundscape.source1;

            if (source != null)
                StartCoroutine(FadeOut(source, musicFadeSpeed));

            _currentLoopingSoundscape = null;
        }
        _currentLoopingSoundscape = s;
        _currentLoopingSoundscapeRoutine = ThemeLoop(s);
        StartCoroutine(_currentLoopingSoundscapeRoutine);
    }

    private void PlayShredder(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (!_shredderPlaying)
        {
            _shredderRoutine = ShredderPlaying(s, true, 1f, true);
            StartCoroutine(_shredderRoutine);
        }
        else
        {
            if (_shredderRoutine != null)
            {
                StopCoroutine(_shredderRoutine);
                _shredderRoutine = ShredderPlaying(s, false, 2f, false);
                StartCoroutine(_shredderRoutine);
            }
        }
    }

    private IEnumerator ShredderPlaying(Sound s, bool fadeIn, float startFadeTime, bool resetVolume)
    {
        _shredderPlaying = true;
        s.activeSource = 0;
        float clipLength = s.source0.clip.length;

        float sourceVolume = s.volume;
        float fadeSpeed = startFadeTime;
        AudioSource source0 = s.source0;
        AudioSource source1 = s.source1;
        int activeSource = 0;

        if (fadeIn)
        {
            if (resetVolume)
                source0.volume = 0f;
            yield return new WaitForSeconds(0.05f);
            source0.Play();
            float time = 0f;
            float targetVolume = s.volume;

            while (source0.volume < targetVolume)
            {
                if (Mathf.Approximately(targetVolume, source1.volume))
                    break;

                source0.volume = Mathf.Clamp(source0.volume + (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            source0.volume = targetVolume;
        }
        else
            source0.Play();

        fadeSpeed = 2f;

        while (true)
        {
            if (activeSource == 0)
            {
                if (clipLength - source0.time < 0.5f)
                {
                    activeSource = 1;
                    source1.volume = 0f;
                    source1.Play();
                    float targetVolume = s.volume;

                    while (source1.volume < targetVolume)
                    {
                        if (Mathf.Approximately(targetVolume, source1.volume))
                            break;

                        source1.volume = Mathf.Clamp(source1.volume + (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                        source0.volume = Mathf.Clamp(source0.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                        yield return new WaitForEndOfFrame();
                    }
                    source1.volume = targetVolume;
                    source0.volume = 0f;
                }
            }
            else if (activeSource == 1)
            {
                if (clipLength - source1.time < 0.5f)
                {
                    activeSource = 0;
                    source0.volume = 0f;
                    source0.Play();
                    float targetVolume = s.volume;

                    while (source0.volume < targetVolume)
                    {
                        if (Mathf.Approximately(targetVolume, source0.volume))
                            break;

                        source0.volume = Mathf.Clamp(source0.volume + (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                        source1.volume = Mathf.Clamp(source1.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                        yield return new WaitForEndOfFrame();
                    }
                    source0.volume = targetVolume;
                    source1.volume = 0f;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void StopShredder(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (_shredderRoutine != null)
        {
            StopCoroutine(_shredderRoutine);
            _shredderRoutine = ShredderStopping(s);
            StartCoroutine(_shredderRoutine);
        }
    }

    private IEnumerator ShredderStopping(Sound s)
    {
        float fadeSpeed = 2f;

        if (s.activeSource == 0)
        {
            float targetVolume = s.volume;

            while (s.source0.volume > 0.01f || s.source1.volume > 0.01f)
            {
                s.source0.volume = Mathf.Clamp(s.source0.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                s.source1.volume = Mathf.Clamp(s.source1.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                yield return new WaitForEndOfFrame();
            }
            s.source0.volume = 0f;
            s.source0.Stop();
            s.source1.volume = 0f;
            s.source1.Stop();
        }
        else if (s.activeSource == 1)
        {
            float targetVolume = s.volume;

            while (s.source0.volume > 0.001f || s.source1.volume > 0.001f)
            {
                s.source0.volume = Mathf.Clamp(s.source0.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                s.source1.volume = Mathf.Clamp(s.source1.volume - (Time.deltaTime * fadeSpeed), 0f, targetVolume);
                yield return new WaitForEndOfFrame();
            }
            s.source0.volume = 0f;
            s.source0.Stop();
            s.source1.volume = 0f;
            s.source1.Stop();
        }
        s.activeSource = 0;
        _shredderPlaying = false;
        _shredderRoutine = null;
    }

    private IEnumerator ThemeLoop(Sound s)
    {
        bool fadeIn = true;

        s.activeSource = 0;
        float clipLength = s.source0.clip.length;
        float fadeSpeed = musicFadeSpeed;

        s.source0.volume = s.volume;
        if (s.source1 != null)
            s.source1.volume = s.volume;

        float sourceVolume = s.volume;
        AudioSource source0 = s.source0;
        AudioSource source1 = s.source1;
        int activeSource = 0;

        if (fadeIn)
            StartCoroutine(FadeIn(sourceVolume, source0, musicFadeSpeed));
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

    private Sound FindSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    public void PlaySoundLoop(string name, float fadeSpeed, bool fadeIn = false)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        LoopingSoundInfo targetInfo = default;
        for (int i = 0; i < _currentLoopingSounds.Count; i++)
        {
            if (_currentLoopingSounds[i].sound == s)
            {
                targetInfo = _currentLoopingSounds[i];
            }
        }

        if (targetInfo.sound == s)
        {
            if (targetInfo.playing)
                return;

            if (targetInfo.loopingRoutine != null)
                StopCoroutine(targetInfo.loopingRoutine);

            targetInfo.playing = true;
            targetInfo.loopingRoutine = LoopSound(s, fadeSpeed, true);
            StartCoroutine(targetInfo.loopingRoutine);
        }
        else
        {
            targetInfo = new LoopingSoundInfo(s, true, LoopSound(s, fadeSpeed, true));
            targetInfo.playing = true;
            _currentLoopingSounds.Add(targetInfo);
            StartCoroutine(targetInfo.loopingRoutine);
        }
    }

    //public void PlaySoundLoop(string name, out Sound s, float fadeSpeed, bool fadeIn = false)
    //{
    //    s = Array.Find(sounds, sound => sound.name == name);
    //    if (s == null)
    //    {
    //        Debug.LogWarning("Sound: " + name + " not found!");
    //        return;
    //    }

    //    LoopingSoundInfo targetInfo = default;
    //    for (int i = 0; i < _currentLoopingSounds.Count; i++)
    //    {
    //        if (_currentLoopingSounds[i].sound == s)
    //        {
    //            targetInfo = _currentLoopingSounds[i];
    //        }
    //    }

    //    if (!targetInfo.Equals(default))
    //    {
    //        if (targetInfo.playing)
    //            return;

    //        StopCoroutine(targetInfo.loopingRoutine);
    //        targetInfo.playing = true;
    //        targetInfo.loopingRoutine = LoopSound(s, fadeSpeed, true);
    //        StartCoroutine(targetInfo.loopingRoutine);
    //    }
    //    else
    //    {
    //        targetInfo = new LoopingSoundInfo(s, true, LoopSound(s, fadeSpeed, true));
    //        targetInfo.playing = true;
    //        _currentLoopingSounds.Add(targetInfo);
    //        StartCoroutine(targetInfo.loopingRoutine);
    //    }
    //}

    private IEnumerator LoopSound(Sound s, float fadeSpeed, bool fadeIn = false)
    {
        s.activeSource = 0;
        float clipLength = s.source0.clip.length;
        if (fadeSpeed < 0.01f)
            fadeSpeed = sfxFadeSpeed;

        s.source0.volume = s.volume;
        if (s.source1 != null)
            s.source1.volume = s.volume;

        float sourceVolume = s.volume;
        AudioSource source0 = s.source0;
        AudioSource source1 = s.source1;
        int activeSource = 0;

        if (fadeIn)
        {
            source0.volume = 0f;
            source0.Play();
            float time = 0f;
            float targetVolume = s.volume;

            while (time < fadeSpeed)
            {
                if (Mathf.Approximately(targetVolume, source1.volume))
                    break;

                source0.volume = Mathf.Clamp(source0.volume + (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            source0.volume = targetVolume;
        }
        else
            source0.Play();

        while (true)
        {
            if (activeSource == 0)
            {
                if (clipLength - source0.time < fadeSpeed)
                {
                    activeSource = 1;
                    source1.volume = 0f;
                    source1.Play();
                    float time = 0f;
                    float targetVolume = s.volume;

                    while (time < fadeSpeed)
                    {
                        if (Mathf.Approximately(targetVolume, source1.volume))
                            break;

                        source1.volume = Mathf.Clamp(source1.volume + (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
                        source0.volume = Mathf.Clamp(source0.volume - (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
                        time += Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    source1.volume = targetVolume;
                    source0.volume = 0f;
                }
            }
            else if (activeSource == 1)
            {
                if (clipLength - source1.time < fadeSpeed)
                {
                    activeSource = 0;
                    source0.volume = 0f;
                    source0.Play();
                    float time = 0f;
                    float targetVolume = s.volume;

                    while (time < fadeSpeed)
                    {
                        if (Mathf.Approximately(targetVolume, source0.volume))
                            break;

                        source0.volume = Mathf.Clamp(source0.volume + (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
                        source1.volume = Mathf.Clamp(source1.volume - (Time.deltaTime / fadeSpeed * targetVolume), 0f, targetVolume);
                        time += Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    source0.volume = targetVolume;
                    source1.volume = 0f;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void StopSoundLoop(string name, bool fadeOut = false)
    {
        LoopingSoundInfo targetInfo = default;

        for (int i = 0; i < _currentLoopingSounds.Count; i++)
        {
            if (_currentLoopingSounds[i].sound.name == name)
                targetInfo = _currentLoopingSounds[i];
        }

        if (targetInfo.loopingRoutine != null)
        {
            StopCoroutine(targetInfo.loopingRoutine);
            targetInfo.playing = false;
            targetInfo.loopingRoutine = StopSFXLoop(name, targetInfo, fadeOut);
            StartCoroutine(targetInfo.loopingRoutine);
        }
    }


    private IEnumerator StopSFXLoop(string name, LoopingSoundInfo targetInfo, bool fadeOut = false)
    {
        AudioSource source0 = targetInfo.sound.source0;
        AudioSource source1 = targetInfo.sound.source1;

        if (fadeOut)
        {
            float time = 0f;
            source0.Play();
            source1.Play();

            while (time < sfxFadeSpeed)
            {
                if (Mathf.Approximately(0f, source0.volume))
                    break;

                source0.volume = Mathf.Clamp(source0.volume - (Time.deltaTime / sfxFadeSpeed), 0f, 1f);
                source1.volume = Mathf.Clamp(source1.volume - (Time.deltaTime / sfxFadeSpeed), 0f, 1f);
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            source0.volume = 0f;
            source1.volume = 0f;
            source0.Stop();
            source1.Stop();
        }
        else
        {
            targetInfo.sound.source0.Stop();
            targetInfo.sound.source1.Stop();
        }
        targetInfo.loopingRoutine = null;
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
        source.volume = 0f;
    }
    #endregion
    #region Tools
    private void ShredderStarted()
    {
        PlayShredder("Shredder_Loop");
    }

    private void ShredderStopped()
    {
        StopShredder("Shredder_Loop");
    }

    private void ScissorsCut()
    {
        PlayRandomSound("Scissor_One", "Scissor_Two", "Scissor_Three");
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
                PlaySound("Drawer_Pull_One");
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
                PlaySound("Drawer_Pull_Two");
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
                PlaySound("Contract_Take");
                break;
            case ObjectType.Scissors:
                PlaySound("Contract_Take");
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
                PlaySound("Contract_Drop");
                break;
            case ObjectType.Scissors:
                PlaySound("Contract_Drop");
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
                //MessageDrop();
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
        //PlaySoundLoop("Writer_Voyant", false, 0f);
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
        PlaySound("ConveyorBelt_Contract_Print");
    }

    private void ContractSpark(MovableContract contract)
    {
        PlayRandomSound("Contract_Spark_One", "Contract_Spark_Two");
    }

    private void NewCreditsReceived()
    {
        switch (_coinIndex)
        {
            case 0:
                PlaySound("Coin_Received_One");
                _coinIndex++;
                break;
            case 1:
                PlaySound("Coin_Received_Two");
                _coinIndex++;
                break;
            case 2:
                PlaySound("Coin_Received_One2");
                _coinIndex++;
                break;
            case 3:
                PlaySound("Coin_Received_Two2");
                _coinIndex = 0;
                break;
        }
    }

    #endregion
    #region Game loop
    private void OnLevelStart()
    {
        _endDay = false;
        Sound s = FindSound("Timer_LastHour_One");
        s.volume = 0.1f;
        Sound s2 = FindSound("Timer_LastHour_Two");
        s2.volume = 0.1f;
    }

    private void LevelOver()
    {
        _endDay = true;
        PlaySound("Timer_DayEnd");
    }

    private void OnFirstAnomaly()
    {
        PlaySound("Anomalie_Spawn");
    }
    
    private void OnClockTick()
    {
        Sound s = FindSound("Timer_LastHour_New");
        s.source0.volume += 0.022f;

        PlaySound("Timer_LastHour_New");


        //Sound s2 = FindSound("Timer_LastHour_Two");
        //s2.volume += 0.08333f;

        //if (_oddClockTick)
        //{
        //    PlaySound("Timer_LastHour_Two");
        //    _oddClockTick = false;
        //}
        //else
        //{
        //    PlaySound("Timer_LastHour_One");
        //    _oddClockTick = true;
        //}
    }

    private void OnUnloading()
    {
        ShredderStopped();
    }

    #endregion


}

public struct LoopingSoundInfo
{
    public Sound sound;
    public bool playing;
    public IEnumerator loopingRoutine;

    public LoopingSoundInfo(Sound sound, bool playing, IEnumerator loopingRoutine)
    {
        this.sound = sound;
        this.playing = playing;
        this.loopingRoutine = loopingRoutine;
    }
}
