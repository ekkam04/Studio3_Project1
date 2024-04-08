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
        public Collider col;
        public Rigidbody rb;
        public Animator anim;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public string[] tagsToIgnore;
        
        public bool dropCoinsOnDeath;
        public int coinsToDrop;

        void Start()
        {
            col = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            // if the object falls off the map
            if (transform.position.y < -30)
            {
                Die();
            }
        }

        public void TakeDamage(int damage, float knockback, Damagable damageDealer)
        {
            if (tagsToIgnore.Length > 0)
            {
                foreach (var tag in tagsToIgnore)
                {
                    if (damageDealer.gameObject.CompareTag(tag)) return;
                }
            }
            
            if (GetComponent<Enemy>() != null)
            {
                GetComponent<Enemy>().attackTimer = 0;
            }
            
            health -= damage;
            OnDamageTaken();
            
            if (health <= 0)
            {
                Die();
            }
            else
            {
                Vector3 damageDealerForward;
                if (damageDealer != null)
                {
                    damageDealerForward = damageDealer.transform.forward;
                }
                else
                {
                    damageDealerForward = Vector3.zero;
                }
                
                print("knockback: " + knockback);

                StartCoroutine(TakeKnockback(damageDealerForward, knockback, 10f, 0.15f));
                
                if (skinnedMeshRenderer != null) StartCoroutine(PulseColor(Color.red, 0.2f, 0.5f));
                if (anim != null) anim.SetTrigger("hit");
            }
        }
        
        public void Heal(int amount)
        {
            if (health + amount > 100)
            {
                health = 100;
            }
            else
            {
                health += amount;
            }
        }
        
        public virtual void OnDamageTaken()
        {
            // this function is meant to be overridden
        }

        public void Die()
        {
            if (dropCoinsOnDeath)
            {
                for (int i = 0; i < coinsToDrop; i++)
                {
                    GameObject coin = Instantiate(GameManager.Instance.coinPrefab, transform.position, Quaternion.identity);
                }
            }
            gameObject.SetActive(false);
        }
        
        // public void TakeKnockback(Vector3 direction, float force)
        // {
        //     rb.AddForce(direction * force);
        // }
        
        IEnumerator TakeKnockback(Vector3 direction, float force, float upForce, float duration)
        {
            float timer = 0;
            while (timer < duration)
            {
                rb.AddForce(direction * force + Vector3.up * upForce);
                timer += Time.deltaTime;
                yield return null;
            }
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
