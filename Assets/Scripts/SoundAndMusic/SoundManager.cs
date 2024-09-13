using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour{

    public static SoundManager instance;
    public static SoundEffects currentSoundEffects;

    private static Transform player;

    public static List<AudioSource> currentPlayedSounds = new List<AudioSource>();
    public static UnityEngine.Audio.AudioMixerGroup mixerGroup;

    [System.Serializable]
    public class SoundEffects{
        public AudioClip jumpSmall;
        public AudioClip jumpBig;
        public AudioClip powerup;
        public AudioClip powerdown;
        public AudioClip throwFireBall;
        public AudioClip probellerFly;
        public AudioClip die;
        public AudioClip hitBlock;
        public AudioClip breakBlock;
        public AudioClip itemPop;
        public AudioClip kicked;
        public AudioClip star;
        public AudioClip coin;
        public AudioClip onOffSwitch;
        public AudioClip oneUp;
        public AudioClip messageBlock;
        public AudioClip explode;/*BowserFire*/
        public AudioClip doorOpen;
        public AudioClip doorClose;
        public AudioClip pSwitch;
        public AudioClip cannon;
        public AudioClip burner;
        public AudioClip hurryUp;
        public AudioClip warpBoxEnter;
        public AudioClip warpBoxExit;
        public AudioClip spin;
        public AudioClip spinKick;
        public AudioClip hitGround;
        public AudioClip jumpTrampoline;
        public AudioClip booLaught;
        public AudioClip checkpoint;
        public AudioClip doorLocked;
        public AudioClip doorUnlock;
    }

    private void Awake(){
        if (instance != null)
            Destroy(this);
        else
            instance = this;

        SoundManager.currentPlayedSounds.Clear();
        player = GameManager.instance.sceneManager.players[0].transform;
        string _OutputMixer = "SoundEffects";
        mixerGroup = SceneManager.audioMixerGroup.audioMixer.FindMatchingGroups(_OutputMixer)[0];
    }

    public static AudioSource PlayAudioClipIfPlayerIsInNear(AudioClip audioClip, Vector3 position){
        if (player.position.x < position.x - 25 | player.position.x > position.x + 25)
            return null;
        return PlayAudioClip(audioClip);
    }

    public static AudioSource PlayAudioClip(AudioClip audioClip){
        if (instance == null)
            return null;
        foreach(AudioSource audioSource in currentPlayedSounds){
            if(audioSource.clip == audioClip){
                audioSource.Stop();
                currentPlayedSounds.Remove(audioSource);
                break;
            }
        }
        

        AudioSource audio = instance.gameObject.AddComponent<AudioSource>();
        audio.outputAudioMixerGroup = mixerGroup;
        audio.clip = audioClip;
        audio.loop = false;
        audio.volume = 1;
        currentPlayedSounds.Add(audio);

        //audio.outputAudioMixerGroup = instance.mixer;

        if (!audio.isPlaying)
            audio.Play();
        instance.StartCoroutine(Audiodestroy(audio, audioClip));
        return audio;
    }

    public static IEnumerator Audiodestroy(AudioSource audio, AudioClip clip){
        bool isPlaying = true;
        while(isPlaying){
            if (audio != null)
                isPlaying = audio.isPlaying;
            else
                isPlaying = false;
            yield return new WaitForSeconds(0);
        }

        currentPlayedSounds.Remove(audio);
        if (audio != null)
            Destroy(audio);
    }

}
