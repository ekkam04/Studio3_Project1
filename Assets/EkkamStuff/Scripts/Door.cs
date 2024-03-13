using UnityEngine;

namespace Ekkam
{
    public class Door : Signalable
    {
        public override void Signal()
        {
            Debug.Log("Door is opening");
        }
    }
}