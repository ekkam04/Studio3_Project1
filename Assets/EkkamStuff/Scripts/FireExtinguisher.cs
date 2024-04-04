using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class FireExtinguisher : MonoBehaviour
    {
        public ParticleSystem smoke;
        public Collider smokeCollider;

        public void Toggle()
        {
            if (smokeCollider.enabled)
            {
                smokeCollider.enabled = false;
                smoke.Stop();
            }
            else
            {
                smokeCollider.enabled = true;
                smoke.Play();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fire"))
            {
                other.GetComponent<Fire>().Extinguish();
            }
        }
    }
}
