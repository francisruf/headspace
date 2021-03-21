using UnityEngine.Audio;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour {
    public Sound[] sounds;
    public static AudioManager instance;
    [HideInInspector]
    public Sound currentMusic;

    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        }

        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        SceneManager.activeSceneChanged += AssignMusicOnScene;
    }

    private void OnEnable()
    {
        ShredderSlot.shredderStarted += ShredderStarted;
        ButtonController.buttonPress += ButtonPress;
        KeyPadButtonController.buttonPress += ButtonPress;
        DropZone_Outbox.commandSuccess += DrawerSuccess;
        DropZone_Outbox.commandFail += DrawerError;
        MovableObject.movableObjectSelected += OnMovableObjectSelected;
        MovableObject.movableObjectDeselected += OnMovableObjectDeselected;
    }

    private void OnDisable()
    {
        ShredderSlot.shredderStarted -= ShredderStarted;
        ButtonController.buttonPress -= ButtonPress;
        KeyPadButtonController.buttonPress -= ButtonPress;
        DropZone_Outbox.commandSuccess -= DrawerSuccess;
        DropZone_Outbox.commandFail -= DrawerError;
        MovableObject.movableObjectSelected -= OnMovableObjectSelected;
        MovableObject.movableObjectDeselected += OnMovableObjectDeselected;
    }

    //Update function only to test feature. Remove when necessary.
   private void Update()
    { 
     //   if (Input.GetKeyDown(KeyCode.S)) 
       // {
        //    PlaySound("Menu_Button_Select");
      //  }
    }
     
    private void AssignMusicOnScene(Scene scene1, Scene scene2) {

        //if (SceneManager.GetActiveScene().name == "MainMenu") {
        //    PlayTheme("MainMenu_Loop");
        //}

        //if (SceneManager.GetActiveScene().name == "MainMenu") {
        //    if (currentMusic.source != null) {
        //        currentMusic.source.Stop();
        //    }
        //}
    }

    public void PlaySound(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Play();
    }

    public void PlayTheme(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (currentMusic.source != null) {
            currentMusic.source.Stop();
        }

        currentMusic = s;
        currentMusic.source.Play();
    }

    private Sound FindSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }


    private void ShredderStarted()
    {
        Sound shredSound = FindSound("Shredder");
        if (shredSound != null)
            if (shredSound.source.isPlaying)
                return;

        PlaySound("Shredder");
    }

    private void ButtonPress()
    {
        int roll = UnityEngine.Random.Range(1, 10);
        if (roll > 5)
        {
            PlaySound("Button_One");
        }
        else
        {
            PlaySound("Button_Two");
        }
    }

    private void DrawerSuccess()
    {
        
    }

    private void DrawerError()
    {
        PlaySound("Error_One");
    }

    private void WriterSignal()
    {
        PlaySound("Writer_Signal");
    }

    private void DrawerOpen()
    {
        
    }

    private void OnMovableObjectSelected(MovableObject obj)
    {
        switch (obj.objectType)
        {
            case ObjectType.Marker:
                break;
            case ObjectType.Document:
                PaperPickup();
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
                break;
            case ObjectType.Document:
                PaperDrop();
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

    private void MenuButtonSelect()
    {
        PlaySound("Menu_Button_Select");
    }
}
