using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam {
    public class PlayerSilhouette : MonoBehaviour
    {

        public Color silhouetteColor;
        float disappearTimer;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public List<Material> materials = new List<Material>();
        public bool fadeAway = true;

        void Awake()
        {
            materials.Add(skinnedMeshRenderer.material);
        }

        void Update()
        {
            if (!fadeAway) return;
            disappearTimer += Time.deltaTime;
            foreach (Material material in materials)
            {
                Color color = silhouetteColor;
                color.a = Mathf.Lerp(0.1f, 0f, disappearTimer * 2.5f);
                material.color = color;
                material.SetColor("_EmissionColor", color);
                material.SetFloat("_EmissionIntensity", Mathf.Lerp(0.5f, 0f, disappearTimer * 2.5f));
            }
            if (disappearTimer >= 1f)
            {
                Destroy(gameObject);
            }
        }

        public void SetSilhouetteColor(Color color)
        {
            silhouetteColor = color;
            silhouetteColor.a = 0.1f;
            foreach (Material material in materials)
            {
                material.color = silhouetteColor;
                material.SetColor("_EmissionColor", color);
                material.SetFloat("_EmissionIntensity", 0.5f);
            }
        }
    }
}