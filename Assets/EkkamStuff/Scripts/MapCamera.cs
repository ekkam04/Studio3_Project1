using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Start()
    {
        // offset = transform.position - target.position;
        // print("Offset: " + offset);
    }

    void Update()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0);
    }
}
