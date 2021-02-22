using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public Sound[] sounds, footsteps;

    // Awake is called when the script instance is being loaded (before Start)
    private void Awake()
    {
        // When our new scene loads, don't delete the audio manager 
        // This is meant to keep the audio manager from the previous scene
        DontDestroyOnLoad(gameObject);

        // Check if this audio manager is the only one in the default scene
        if (instance == null)
        {
            // If this is the only audio manager, then we can keep it
            instance = this;
        }
        else
        {
            // If we don't have another audio manager, destroy the one in the default scene
            Destroy(gameObject);
        }

        // Loop through each sound placed in the audio manager, 
        //  create an audio source for each sound (allows us to call the sound),
        //  and assign all the details we know about each sound to their audio sources
        // Loop through each sound placed in the audio manager, 
        //  create an audio source for each sound (allows us to call the sound),
        //  and assign all the details we know about each sound to their audio sources
        foreach (Sound sound in sounds)
        {
            // Create an audio source for the current sound
            sound.audioSource = gameObject.AddComponent<AudioSource>();

            // Assign all the details we know about the current sound to the audio source
            sound.audioSource.clip = sound.audioClip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
        }
        foreach (Sound sound in footsteps)
        {
            // Create an audio source for the current sound
            sound.audioSource = gameObject.AddComponent<AudioSource>();

            // Assign all the details we know about the current sound to the audio source
            sound.audioSource.clip = sound.audioClip;
            sound.audioSource.volume = sound.volume;
            sound.audioSource.pitch = sound.pitch;
            sound.audioSource.loop = sound.loop;
        }
    }

    // Play the sound being requested based on the name passed through
    public void PlaySound(string name)
    {
        Sound requestedSound = Array.Find(sounds, sound => sound.name == name);

        // See if the sound we want to play exists
        if (requestedSound == null)
        {
            Debug.LogWarning("The sound '" + name + "' does not exist");
            return;
        }
        else
        {
            requestedSound.audioSource.Play();
        }
    }

    // Stop the sound being requested based on the name passed through
    public void StopSound(string name)
    {
        Sound requestedSound = Array.Find(sounds, sound => sound.name == name);

        // See if the sound we want to stop exists
        if (requestedSound == null)
        {
            Debug.LogWarning("The sound '" + name + "' does not exist");
            return;
        }
        else
        {
            requestedSound.audioSource.Stop();
        }
    }

    // Check if the requested sound is playing based on the name passed through
    public bool CheckSound(string name)
    {
        Sound requestedSound = Array.Find(sounds, sound => sound.name == name);

        // See if the sound we want to check exists
        if (requestedSound == null)
        {
            Debug.LogWarning("The sound '" + name + "' does not exist");
            return false;
        }
        else
        {
            return requestedSound.audioSource.isPlaying;
        }
    }

    // Returns a random footstep sound
    public Sound GetRandomFootstep()
    {
        return footsteps[UnityEngine.Random.Range(0, footsteps.Length)];
    }
}
