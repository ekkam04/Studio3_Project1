using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition3D : MonoBehaviour
{
    public Camera mainCamera;
    
    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            transform.position = hit.point;
        }
    }
}
