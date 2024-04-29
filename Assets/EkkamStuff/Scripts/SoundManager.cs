using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        public List<SoundCue> soundCues = new List<SoundCue>();
        public AudioSource defaultSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void PlaySound(string name, AudioSource source = null)
        {
            SoundCue soundCue = soundCues.Find(sound => sound.name == name);
            if (soundCue == null)
            {
                Debug.LogWarning("Sound cue not found: " + name);
                return;
            }

            if (source == null)
            {
                source = defaultSource;
            }
            
            source.volume = soundCue.volume;
            source.PlayOneShot(soundCue.clips[UnityEngine.Random.Range(0, soundCue.clips.Length - 1)]);
        }
    }
    
    [System.Serializable]
    public class SoundCue
    {
        public string name;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
    }
}