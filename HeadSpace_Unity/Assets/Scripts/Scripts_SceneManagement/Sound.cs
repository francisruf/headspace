﻿using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]

public class Sound {

    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;
    [Range(-1f, 1f)]
    public float stereoPan;
    [HideInInspector]
    public float loopVolume;

    public bool loop;
    public int activeSource;

    [HideInInspector]
    public AudioSource source0;
    [HideInInspector]
    public AudioSource source1;
}
