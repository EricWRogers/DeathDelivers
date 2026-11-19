using UnityEngine;
using UnityEngine.InputSystem;

public class TrafficLightDestruction : MonoBehaviour
{
    [Header("Traffic Light Pieces")]
    [SerializeField] private Rigidbody pole;
    [SerializeField] private Rigidbody signalHead;

    [Header("Destruction Settings")]
    [SerializeField] private float breakForce = 8f;

    private bool isBroken = false;


    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.kKey.wasPressedThisFrame)
        {
            BreakTrafficLight(Vector3.right);
        }
    }


    public void BreakTrafficLight(Vector3 direction)
    {
        if (isBroken) return;

        isBroken = true;

        // Detach the pieces from their parent
        pole.transform.SetParent(null, true);
        signalHead.transform.SetParent(null, true);

        // Enable physics
        pole.isKinematic = false;
        signalHead.isKinematic = false;

        // Apply le forces in different directions
        pole.AddForce(
            direction.normalized * breakForce,
            ForceMode.Impulse
        );

        signalHead.AddForce(
            (direction + Vector3.up).normalized * breakForce,
            ForceMode.Impulse
        );

        // Make the signal head tumble
        signalHead.AddTorque(
            Random.onUnitSphere * breakForce,
            ForceMode.Impulse
        );
    }
}