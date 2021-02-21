using System;
using UnityEngine;

// A class for storing the details of a sound
[Serializable]
public class Sound
{
    public AudioClip audioClip;
    public string name;
    [Range(0f, 1f)]
    public float volume;
    [Range(0f, 1f)]
    public float pitch;
    public bool loop;
    [HideInInspector]
    public AudioSource audioSource;
}
