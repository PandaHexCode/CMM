using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour{

    public static BackgroundMusicManager instance;
        
    public AudioSource playModeSource;
    public AudioSource editModeSource;

    private void Awake(){
        if (instance != null)
            Destroy(this);
        else
            instance = this;
    }

    public void StartPlayingBackgroundMusic(){
        if (this.playModeSource.isPlaying | GameManager.instance.isInMainMenu)
            return;
        this.playModeSource.clip = TileManager.instance.currentTileset.playModeBGM;
        this.editModeSource.clip = TileManager.instance.currentTileset.editModeBGM;
        if (this.editModeSource.clip == null){
            this.editModeSource.clip = this.playModeSource.clip;
            this.editModeSource.outputAudioMixerGroup = SceneManager.audioMixerGroup.audioMixer.FindMatchingGroups("MusicEditorForNoExtraMusic")[0];
        }else
            this.editModeSource.outputAudioMixerGroup = SceneManager.audioMixerGroup.audioMixer.FindMatchingGroups("Music")[0];

        this.playModeSource.Play();
        this.editModeSource.Play();
    }

    public void StopCurrentBackgroundMusic(){
        if (!this.playModeSource.isPlaying)
            return;

        this.playModeSource.Stop();
        this.editModeSource.Stop();
    }

    public void StartListingToEditSource(){
        if (GameManager.instance.isInMainMenu){
            this.playModeSource.volume = 0;
            this.editModeSource.volume = 0;
            return;
        }
        this.playModeSource.volume = 0;
        this.editModeSource.volume = 1;
    }

    public void StartListingToPlayModeSource(){
        if (GameManager.instance.isInMainMenu){
            this.playModeSource.volume = 0;
            this.editModeSource.volume = 0;
            return;
        }

        this.playModeSource.volume = 1;
        this.editModeSource.volume = 0;
    }

}
