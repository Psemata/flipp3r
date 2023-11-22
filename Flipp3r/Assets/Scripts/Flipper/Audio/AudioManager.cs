using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {

    // Singleton instance
    public static AudioManager Instance;

    // All the sounds
    public Sound[] sounds;

    // Bool used to manage specific sounds
    public bool shellSoundPlayed = false;
    private bool gameplayMusicPlayed = false;
    private bool gameplayLoopMusicPlayed = false;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        foreach(Sound s in this.sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Update() {
        // Check if the gameplay music is finished, and start the loop one if it is
        if(this.gameplayMusicPlayed && !this.gameplayLoopMusicPlayed && !IsPlaying("musique-gameplay") && GameManager.Instance.State == GameState.Game) {
            Play("musique-gameplay-loop");
            this.gameplayLoopMusicPlayed = true;
        }
    }

    // Play the corresponding song
    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null) {
            Debug.LogWarning("Sound : " + name + " not found !");
            return;
        }
        s.source.Play();
    }

    // Gameplay music
    public void PlayGameplayMusic() {
        Play("musique-gameplay");
        this.gameplayMusicPlayed = true;
    }

    // Gameover music
    public void PlayGameOverMusic() {
        StopGameplayMusic();
        Play("musique-gameover");
    }

    // Victory music
    public void PlayVictoryMusic() {
        Debug.Log(GameManager.Instance.State);
        Play("musique-victory");
        Debug.Log(GameManager.Instance.State);
    }

    // Boss sounds
    public void PlayHitSound(int index) {
        Sound s = this.sounds[index];
        Debug.Log(s.name);
        s.source.Play();
    }

    public void PlayTauntSound(int index) {
        Sound s = this.sounds[10 + index];
        Debug.Log(s.name);
        s.source.Play();
    }

    public void PlayDestructionSound(int index) {
        Sound s = this.sounds[16 + index];
        Debug.Log(s.name);
        s.source.Play();
    }

    // Shell sound
    public void ShellSound() {
        if(!this.shellSoundPlayed) {
            Play("shells");
            this.shellSoundPlayed = true;
        }
    }

    // Stop the corresponding song
    public void StopPlaying(string sound) {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Stop();
    }

    public void StopGameplayMusic() {
        if(IsPlaying("musique-gameplay-loop")) {
            StopPlaying("musique-gameplay-loop");
        } else {
            StopPlaying("musique-gameplay");
        }
    }

    // Fade the music out by its name
    IEnumerator FadeMusic(string sound) {
        Sound s = Array.Find(sounds, item => item.name == sound);
        if (s == null) {
            Debug.LogWarning("Sound: " + name + " not found!");
            yield break;
        }

        while(s.source.volume >= 0) {
            s.source.volume -= 0.00001f;
            yield return new WaitForSeconds(0.01f);
        }

        s.source.Stop();
    }

    // Fade the music out
    IEnumerator FadeMusic(Sound s) {
        Debug.Log(s.source.volume);
        while(s.source.volume >= 0) {
            Debug.Log(s.source.volume);
            s.source.volume -= 0.00001f;
            yield return new WaitForSeconds(0.01f);
        }

        s.source.Stop();
    }

    // Stop all the musics
    public void StopPlayingAll(){
        foreach(Sound s in sounds){
            s.source.Stop();
        }
    }

    // Fade all the musics out
    public void FadingAll() {
        foreach(Sound s in sounds){
            if(s.name == "musique-victory") {
                continue;
            }
            StartCoroutine(FadeMusic(s));
        }
    }

    // Check if the corresponding song is playing
    public bool IsPlaying(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null) {
            Debug.LogWarning("Sound : " + name + " not found !");
            return false;
        }
        return s.source.isPlaying;
    }
}
