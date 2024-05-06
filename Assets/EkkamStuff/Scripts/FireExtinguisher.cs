using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Ekkam
{
    public class FireExtinguisher : MonoBehaviour
    {
        public ParticleSystem smoke;
        public Collider smokeCollider;
        private AudioSource audioSource;
        public AudioClip smokeSound;
        
        private void Start()
        {
            audioSource = transform.AddComponent<AudioSource>();
            audioSource.volume = 0.5f;
            audioSource.clip = smokeSound;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.Stop();
        }

        public void Toggle()
        {
            if (smokeCollider.enabled)
            {
                smokeCollider.enabled = false;
                smoke.Stop();
                audioSource.Stop();
            }
            else
            {
                smokeCollider.enabled = true;
                smoke.Play();
                audioSource.Play();
            }
        }
        
        public void StartSmoke()
        {
            smokeCollider.enabled = true;
            smoke.Play();
            audioSource.Play();
        }
        
        public void StopSmoke()
        {
            smokeCollider.enabled = false;
            smoke.Stop();
            audioSource.Stop();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fire"))
            {
                other.GetComponent<Fire>().Extinguish();
                SoundManager.Instance.PlaySound("staff-shoot");
            }
        }
    }
}
