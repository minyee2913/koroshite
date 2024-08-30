using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set;}
    public PhotonView pv;
    public Sound[] Sounds;
    public AudioSource[] _tracks;

    public string PlayingMusic;

    void Start() {
        
        Instance = this;

        pv = GetComponent<PhotonView>();
    }

    public void PlayToAll(string musicId, bool seper = false) {
        pv.RpcSecure("Play", RpcTarget.All, true, new object[]{musicId, seper});
    }

    public void PlayToDist(string musicId, Vector3 pos, float distance) {
        pv.RpcSecure("playTo", RpcTarget.Others, true, new object[]{musicId, pos.x, pos.y, distance});
        playTo(musicId, pos.x, pos.y, distance);
    }

    [PunRPC]
    public void playTo(string musicId, float x, float y, float distance) {
        if (Vector2.Distance(Player.Local.transform.position, new Vector2(x, y)) <= distance) {
            Play(musicId, true);
        }
    }

    [PunRPC]
    public void Play(string musicId, bool seper = false)
    {
        Debug.Log("Play");
        Sound sound = Array.Find(Sounds, v => v.id == musicId);
        if (sound == null) return;

        StartCoroutine(OnPlay(sound, seper));
    }

    public void StopToAll(int track) {
        pv.RpcSecure("Stop", RpcTarget.All, true, new object[]{track});
    }

    [PunRPC]
    public void Stop(int track)
    {
        _tracks[track - 1].Stop();
        PlayingMusic = null;
    }

    public void Pause(int track)
    {
        _tracks[track - 1].Pause();
    }

    public void UnPause(int track)
    {
        if (_tracks[track - 1]) _tracks[track - 1].UnPause();
    }

    public IEnumerator OnPlay(Sound sound, bool seper)
    {
        if (sound.track == 4)
        {
            if (PlayingMusic != null && PlayingMusic == sound.id) yield break;
            else PlayingMusic = sound.id;
        }
        AudioSource _audio = _tracks[sound.track - 1];

        if (seper) {
            _audio = Instantiate(_audio);
            Destroy(_audio.gameObject, sound.audio.length);
        }

        if (sound.audioIn)
        {
            _audio.clip = sound.audioIn;
            _audio.loop = false;
            _audio.volume = sound.volume;
            _audio.pitch = sound.pitch;
            _audio.time = sound.startTime;
            _audio.Play();

            while (true)
            {

                yield return new WaitForSecondsRealtime(0.01f);
                if (!_audio.isPlaying)
                {
                    _audio.clip = sound.audio;
                    _audio.Play();
                    _audio.loop = sound.loop;

                    break;
                }
            }
        }
        else
        {
            _audio.clip = sound.audio;
            _audio.loop = sound.loop;
            _audio.volume = sound.volume;
            _audio.pitch = sound.pitch;
            _audio.time = sound.startTime;
            _audio.Play();
        }
    }
}
