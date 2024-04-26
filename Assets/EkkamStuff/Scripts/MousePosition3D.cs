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
        
        public bool scanCollider;
        public GameObject currentScannable;
        private Scannable[] scannables;
        
        void Start()
        {
            scannables = FindObjectsOfType<Scannable>();
        }

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
                if (scanCollider)
                {
                    if (hit.collider.GetComponent<Scannable>() && hit.collider.gameObject != currentScannable)
                    {
                        if (currentScannable)
                        {
                            currentScannable.GetComponent<Scannable>().outline.enabled = false;
                        }
                        currentScannable = hit.collider.gameObject;
                        var scannable = hit.collider.GetComponent<Scannable>();
                        scannable.outline.enabled = true;
                    }
                    else if (!hit.collider.GetComponent<Scannable>() && currentScannable)
                    {
                        currentScannable.GetComponent<Scannable>().outline.enabled = false;
                        currentScannable = null;
                    }
                    else if (hit.collider.GetComponent<Scannable>() && hit.collider.gameObject == currentScannable)
                    {
                        return;
                    }
                    else
                    {
                        foreach (var scannable in scannables)
                        {
                            scannable.outline.enabled = false;
                        }
                    }
                }
            }
        }
    }
}
