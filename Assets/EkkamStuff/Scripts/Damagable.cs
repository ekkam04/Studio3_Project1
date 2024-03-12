using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine;
using UnityEngine.UI;

namespace Ekkam
{
    public class Damagable : MonoBehaviour
    {
        public int health = 1;
        public float selfKnockbackForce = 1;
        public Collider col;
        public Rigidbody rb;
        public Animator anim;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        void Start()
        {
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        public void TakeDamage(int damage, Vector3 damageDealerForward)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
            else
            {
                TakeKnockback(damageDealerForward, selfKnockbackForce);
                if (skinnedMeshRenderer != null) StartCoroutine(PulseColor(Color.red, 0.2f, 0.5f));
                if (anim != null) anim.SetTrigger("hit");
            }
        }

        public void Die()
        {
            gameObject.SetActive(false);
        }
        
        public void TakeKnockback(Vector3 direction, float force)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
        
        IEnumerator PulseColor(Color color, float fadeInDuration, float fadeOutDuration)
        {
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                
                skinnedMeshRenderer.material.color = Color.Lerp(Color.white, color, t / fadeInDuration);
                
                yield return null;
            }
            for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
            {
                
                skinnedMeshRenderer.material.color = Color.Lerp(color, Color.white, t / fadeOutDuration);
                
                yield return null;
            }
        }
    }
}
