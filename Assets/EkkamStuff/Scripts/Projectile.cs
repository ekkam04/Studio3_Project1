using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class Projectile : MonoBehaviour
    {
        Collider col;
        public float speed = 10f;
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
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, projectileDirection);
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, projectileDirection);
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, projectileDirection);
            }
            if (destroyOnHit) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision other)
        {
            print("Hit " + other.gameObject.name);
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage, projectileDirection);
            }
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
