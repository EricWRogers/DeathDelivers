using UnityEngine;

public class PersonBehavior : MonoBehaviour
{
    public Camera camera;
    public float speed = 1.0f;
    public Transform body;
    public float joyAndWhimsy = 0.07f;
    private float jaWtim = 0f;
    private bool isUp = true;

    void Start()
    {
        if (camera == null)
            camera = Camera.main;
    }
    void Update()
    {
        body.Translate(Vector3.forward * speed * Time.deltaTime);
        jaWtim += Time.deltaTime;
        if (joyAndWhimsy <= jaWtim)
        {
            if (isUp)
                isUp = false;
            else
                isUp = true;
        }
        else
        {
            if (isUp)
                transform.Translate(Vector3.up * 3.0f * Time.deltaTime);
            else
                transform.Translate(Vector3.down * 3.0f * Time.deltaTime);
        }
    }

    void LateUpdate()
    {
        // Make the object look at the camera
        transform.LookAt(camera.transform);
        // Get current rotation in Euler angles
        Vector3 currentRotation = transform.eulerAngles;

        // Add 90 degrees to the X axis
        currentRotation.x += 90f;

        // Apply the new rotation
        transform.eulerAngles = currentRotation;
    }
}