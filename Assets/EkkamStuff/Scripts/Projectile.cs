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

        void Start()
        {
            col = GetComponent<Collider>();
        }

        void Update()
        {
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            print("Hit " + other.gameObject.name);
            if (other.gameObject.GetComponent<Damagable>())
            {
                other.gameObject.GetComponent<Damagable>().TakeDamage(damage);
                if (destroyOnHit) Destroy(gameObject);
            }
        }
    }
}
