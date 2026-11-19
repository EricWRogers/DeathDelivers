
using UnityEngine;

public class TrafficLightCollision : MonoBehaviour
{
    [SerializeField] private TrafficLightDestruction destruction;
    [SerializeField] private float minimumImpactSpeed = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        // Ignore weak collisions ^^
        if (collision.relativeVelocity.magnitude < minimumImpactSpeed)
            return;

        Vector3 direction = collision.relativeVelocity;

        // Break the traffic light
        destruction.BreakTrafficLight(direction);
    }
}
