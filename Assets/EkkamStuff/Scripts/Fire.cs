using UnityEngine;

namespace Ekkam
{
    public class Fire : MonoBehaviour
    {
        public ParticleSystem fire;
        public Collider fireCollider;
        
        public int damage = 1;
        private float timer = 0;
        public float damageInterval = 1;
        
        public Signalable signalOnExtinguish;
        
        private void Start()
        {
            timer = damageInterval + 1f;
        }
        
        public void Ignite()
        {
            fire.Play();
            fireCollider.enabled = true;
        }
        
        public void Extinguish()
        {
            fire.Stop();
            fireCollider.enabled = false;
            if (signalOnExtinguish != null) signalOnExtinguish.Signal();
            Invoke(nameof(DisableFire), 2);
        }

        private void DisableFire()
        {
            this.gameObject.SetActive(false);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            {
                timer += Time.deltaTime;
                if (timer >= damageInterval)
                {
                    timer = 0;
                    other.GetComponent<Damagable>().TakeDamage(damage, 1, null);
                }
            }
        }
    }
}