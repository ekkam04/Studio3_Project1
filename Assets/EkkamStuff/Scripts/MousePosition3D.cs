using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkam
{

    public class MousePosition3D : MonoBehaviour
    {
        public Camera mainCamera;

        public bool observeCollider;
        public Collider colliderToObserve;

        void Update()
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                transform.position = hit.point;
                if (observeCollider)
                {
                    if (hit.collider == colliderToObserve)
                    {
                        var objectiveManager = FindObjectOfType<ObjectiveManager>();
                        objectiveManager.playerObservedColliderCheck = true;
                        observeCollider = false;
                    }
                }
            }
        }
    }
}
