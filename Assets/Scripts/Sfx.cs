using System.Collections.Generic;
using UnityEngine;

// Plays short sound effects stored in Assets/Resources/Audio.
// No scene setup needed: the AudioSource is created the first time a sound plays.
public static class Sfx
{
    private static AudioSource source;
    private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    public static void Play(string name, float volume = 1f)
    {
        AudioClip clip;
        if (!clips.TryGetValue(name, out clip))
        {
            clip = Resources.Load<AudioClip>("Audio/" + name);
            clips[name] = clip;
            if (clip == null) Debug.LogWarning("Sound not found: Resources/Audio/" + name);
        }
        if (clip == null) return;

        if (source == null)
        {
            GameObject go = new GameObject("SFX");
            source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
        }
        source.pitch = Random.Range(0.94f, 1.06f); // small variation so repeated hits don't sound identical
        source.PlayOneShot(clip, volume);
    }
}
