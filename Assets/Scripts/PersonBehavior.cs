using UnityEngine;

public class PersonBehavior : MonoBehaviour
{
    public Camera camera; // optional, defaults to Camera.main

    void Start()
    {
        if (camera == null)
            camera = Camera.main;
    }

    void LateUpdate()
    {
        // Make the object look at the camera
        transform.LookAt(camera.transform + (transform.localRotation.x + 90));

        // Optional: lock vertical rotation to avoid flipping
        // transform.localRotation = Quaternion.Euler(90f, transform.rotation.y, 0);
    }
}