using UnityEngine;

public class CarNode : MonoBehaviour
{
    public Transform nextNode;

    private void OnTriggerEnter(Collider other)
    {
        CarNPCMove car = other.GetComponent<CarNPCMove>();

        if (car == null)
            return;

        if (nextNode != null)
        {
            car.targetPoint = nextNode;
        }
        else
        {
            Destroy(car.gameObject);
        }
    }
}
