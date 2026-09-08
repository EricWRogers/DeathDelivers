using UnityEngine;

public class soulIncrease : MonoBehaviour

{
    public DogMovement dog; 
    //This makes you have to manually assign which object is the dog 
    // No idea if there is an easier way to do this
    
    [Tooltip("The amount the soul count is increased by")]
    public float soulValue = 1; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
           dog.soulCount += soulValue;
           Destroy(gameObject); 
        }
    }
}
