using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Projectile : MonoBehaviour
    {
        public Damagable projectileOwner;
        public float speed = 10f;
        public float fallOffSpeed = 0f;
        public int damage = 1;
        public float knockback = 100;
        public bool destroyOnHit = true;
        public bool freezeOnHit = false;
        
        public bool isMelee = false;
        public GameObject baseParent;
        
        [Header("Particle Settings")]
        public ParticleSystem trailParticleSystem;
        public float trailStartDelay = 0.5f;
        
        private void Start()
        {
            if (trailParticleSystem != null)
            {
                Invoke("EnableTrail", trailStartDelay);
            }
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            transform.Translate(Vector3.down * fallOffSpeed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            print("Hit " + other.gameObject.name);
            
            if (freezeOnHit)
            {
                speed = 0;
            }
            
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, knockback, projectileOwner);
            }
            else if (other.gameObject.GetComponent<Interactable>() != null && other.gameObject.GetComponent<Interactable>().enabled && other.gameObject.GetComponent<Interactable>().interactionType == Interactable.InteractionType.Damage)
            {   
                other.gameObject.GetComponent<Interactable>().Interact();
            }
            if (destroyOnHit) Destroy(gameObject);
        }
        
        private void EnableTrail()
        {
            trailParticleSystem.Play();
        }

        // private void OnCollisionEnter(Collision other)
        // {
        //     print("Hit " + other.gameObject.name);
        //     if (other.gameObject.GetComponent<Damagable>())
        //     {
        //         other.gameObject.GetComponent<Damagable>().TakeDamage(damage, this.gameObject, projectileDirection, knockback, knockup);
        //     }
        //     if (destroyOnHit) Destroy(gameObject);
        //     if (freezeOnHit) speed = 0;
        // }
    }
}
