using System;
using UnityEngine;

namespace Ekkam
{
    public class Collectible : MonoBehaviour
    {
        ParticleSystem particleSystem;
        
        public enum CollectibleType
        {
            Coin,
            Token
        }
        public CollectibleType collectibleType;
        
        void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
            particleSystem.Play();
            Invoke(nameof(EnableExternalForces), 0.5f);
        }
        
        void EnableExternalForces()
        {
            var externalForces = particleSystem.externalForces;
            externalForces.enabled = true;
        }

        private void OnParticleCollision(GameObject other)
        {
            print("Collided with particle system");
            if (other.CompareTag("Player"))
            {
                switch (collectibleType)
                {
                    case CollectibleType.Coin:
                        Player.Instance.coins++;
                        SoundManager.Instance.PlaySound("coin-collect");
                        break;
                    case CollectibleType.Token:
                        Player.Instance.tokens++;
                        SoundManager.Instance.PlaySound("token-collect");
                        break;
                }
                particleSystem.Stop();
                Destroy(gameObject);
            }
        }
    }
}