using UnityEngine;

public class KnockableTrafficLight : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void KnockOver(Vector3 direction, Vector3 hitpoiint)
    {
        if (rb == null) return;

        rb.isKinematic = false;
        rb.AddForceAtPosition(direction, hitpoiint, ForceMode.Impulse);

    }
}
