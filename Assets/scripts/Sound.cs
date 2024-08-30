using UnityEngine;

[System.Serializable]
public class Sound
{
    public string id;
    public AudioClip audioIn;
    public AudioClip audio;
    public float volume = 1;
    public float pitch = 1;
    public float startTime = 0;

    [Range(1f, 4f)]
    public int track = 1;

    public bool loop = false;
}
