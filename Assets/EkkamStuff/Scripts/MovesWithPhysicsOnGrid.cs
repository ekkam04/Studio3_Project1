using System;
using UnityEngine;

namespace Ekkam
{
    public class MovesWithPhysicsOnGrid : MonoBehaviour
    {
        public delegate void OnMoveComplete();
        public static event OnMoveComplete onMoveComplete;
        
        public Rigidbody rb;
        bool isMoving;
        
        public float magnitude = 1;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }
        
        // when the rigidbody finishes moving, call onMoveComplete and wait for the next move
        void Update()
        {
            magnitude = rb.velocity.magnitude;
            
            if (isMoving && rb.velocity.magnitude < 1f)
            {
                isMoving = false;
                if (onMoveComplete != null)
                {
                    onMoveComplete();
                }
            }
            
            if (rb.velocity.magnitude > 1f)
            {
                isMoving = true;
            }
        }

        private void OnDestroy()
        {
            if (onMoveComplete != null)
            {
                onMoveComplete();
            }
        }
    }
}