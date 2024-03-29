using UnityEngine;

namespace Ekkam
{
    public class Wire : Signalable
    {
        public Signalable connectedSignalable;
        public Signalable[] extraConnectedSignalables;
        public Wire prerequisiteWire;
        
        public bool signalSent;
        public bool isPowered;
        
        public Color poweredColor;
        
        private Material wireMaterial;
        
        void Start()
        {
            wireMaterial = GetComponent<MeshRenderer>().material;
        }
        
        void Update()
        {
            if (connectedSignalable != null && isPowered && !signalSent)
            {
                connectedSignalable.Signal();
                foreach (var signalable in extraConnectedSignalables)
                {
                    signalable.Signal();
                }
                signalSent = true;
            }
        }
        
        public override void Signal()
        {
            if (prerequisiteWire != null && !prerequisiteWire.isPowered)
            {
                return;
            }
            
            isPowered = true;
            wireMaterial.color = poweredColor;
            wireMaterial.SetColor("_EmissionColor", poweredColor);
            wireMaterial.EnableKeyword("_EMISSION");
        }
    }
}