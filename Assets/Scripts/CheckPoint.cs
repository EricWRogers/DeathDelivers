using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    private void OnTriggerEnter(Collider other){
        DogMovement dog = other.GetComponentInParent<DogMovement>();
            if(dog != null){
                Destroy(gameObject);
        }
    }
}
