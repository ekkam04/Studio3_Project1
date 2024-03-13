using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Projectile : MonoBehaviour
    {
        Collider col;
        public float speed = 10f;
        public float fallOffSpeed = 0f;
        public int damage = 1;
        public bool destroyOnHit = true;
        public bool freezeOnHit = false;
        
        public bool isMelee = false;
        public GameObject baseParent;
        
        private Vector3 projectileDirection;

        void Start()
        {
            col = GetComponent<Collider>();
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
            transform.Translate(Vector3.down * fallOffSpeed * Time.deltaTime);
            
            if (isMelee)
            {
                projectileDirection = baseParent.transform.forward;
            }
            else
            {
                projectileDirection = transform.forward;
            }
            
        }

        void OnTriggerEnter(Collider other)
        {
            print("Hit " + other.gameObject.name);
            if (freezeOnHit)
            {
                speed = 0;
                // set parent to other without changing position and scale
                // transform.parent = other.transform;
            }
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, this.gameObject, projectileDirection);
            }
            else if (other.gameObject.GetComponent<Interactable>() != null && other.gameObject.GetComponent<Interactable>().interactionType == Interactable.InteractionType.Damage)
            {   
                other.gameObject.GetComponent<Interactable>().Interact();
            }
            if (destroyOnHit) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            print("Hit " + other.gameObject.name);
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, this.gameObject, projectileDirection);
            }
            if (destroyOnHit) Destroy(gameObject);
            if (freezeOnHit) speed = 0;
        }

        void Collide(Collision other)
        {
            print("Hit " + other.gameObject.name);
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, this.gameObject, projectileDirection);
            }
            if (destroyOnHit) Destroy(gameObject);
            if (freezeOnHit) speed = 0;
        }
    }
}
