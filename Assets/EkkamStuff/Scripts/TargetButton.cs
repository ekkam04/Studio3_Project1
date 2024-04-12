using System;
using UnityEngine;

namespace Ekkam
{
    public class TargetButton : Signalable
    {
        public bool isPowered = false;
        public Material[] materials; // 0 = off, 1 = on

        private void Start()
        {
            Power(isPowered);
        }

        public override void Signal()
        {
            Power(!isPowered);
        }
        
        private void Power(bool turnOn)
        {
            isPowered = turnOn;
            var newMatInstance = new Material(materials[turnOn ? 1 : 0]);
            if (turnOn)
            {
                newMatInstance.EnableKeyword("_EMISSION");
            }
            else
            {
                newMatInstance.DisableKeyword("_EMISSION");
            }
            GetComponent<MeshRenderer>().material = newMatInstance;
            if (GetComponent<Interactable>() != null) GetComponent<Interactable>().enabled = turnOn;
        }
    }
}