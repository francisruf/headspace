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

    //Update function only to test feature. Remove when necessary.
    private void Update() {
        if (Input.GetKeyDown(KeyCode.S)) {
            PlaySound("Yes.");
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
}
