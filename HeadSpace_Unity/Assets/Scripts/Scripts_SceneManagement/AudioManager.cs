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
        DropZone_Outbox.commandSuccess += CommandSuccess;
        DropZone_Outbox.commandFail += CommandFail;
    }

    private void OnDisable()
    {
        ShredderSlot.shredderStarted -= ShredderStarted;
        ButtonController.buttonPress -= ButtonPress;
        KeyPadButtonController.buttonPress -= ButtonPress;
        DropZone_Outbox.commandSuccess -= CommandSuccess;
        DropZone_Outbox.commandFail -= CommandFail;
    }

    //Update function only to test feature. Remove when necessary.
   private void Update() { 
  if (Input.GetKeyDown(KeyCode.S)) 
     {
    PlaySound("Paper_Pickup");
    }
 
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

    private void CommandFail()
    {
        
    }

    private void CommandSuccess()
    {

    }

    private void AnomalyWarning()
    {

    }

    private void ShredderStarted()
    {
        PlaySound("Shredder");
    }

    private void ButtonPress()
    {
        PlaySound("Button_One");
    }

    private void ButtonPressTwo()
    {
        PlaySound("Button_Two");
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

    private void PaperPickUp()
    {

    }

    private void PaperDrop()
    {

    }
}
