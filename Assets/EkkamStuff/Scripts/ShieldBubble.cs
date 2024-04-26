using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{
    public class ShieldBubble : MonoBehaviour
    {
        public Player player;
        [SerializeField] Material shieldMaterial;
        public float shieldAlpha;

        private void Start()
        {
            shieldMaterial = Instantiate(GetComponent<MeshRenderer>().material);
            GetComponent<MeshRenderer>().material = shieldMaterial;
        }

        public void OnTriggerEnter(Collider other)
        {
            player.energy -= player.shieldEnergyDrainOnHit;
            if (player.energy < 0)
            {
                player.energy = 0;
            }
            else
            {
                StartCoroutine(PulseAlpha(0.1f, 0.25f));
            }
        }

        IEnumerator PulseAlpha(float fadeInTime, float fadeOutTime)
        {
            float t = 0;
            while (t < fadeInTime)
            {
                shieldAlpha = Mathf.Lerp(0.64f, 0.04f, t / fadeInTime);
                shieldMaterial.SetFloat("_EdgeThickness", shieldAlpha);
                t += Time.deltaTime;
                yield return null;
            }

            t = 0;
            while (t < fadeOutTime)
            {
                shieldAlpha = Mathf.Lerp(0.04f, 0.64f, t / fadeOutTime);
                shieldMaterial.SetFloat("_EdgeThickness", shieldAlpha);
                t += Time.deltaTime;
                yield return null;
            }
        }
    }
}
