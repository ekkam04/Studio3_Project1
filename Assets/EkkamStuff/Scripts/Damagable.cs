using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class Damagable : MonoBehaviour
    {
        public int health = 1;
        public Collider col;

        void Start()
        {
            col = GetComponent<Collider>();
        }

        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0) Die();
        }

        public void Die()
        {
            Destroy(gameObject);
        }
    }
}
